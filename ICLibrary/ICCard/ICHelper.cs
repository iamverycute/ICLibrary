using System;
using RFIDLIB;
using Ionic.Zip;
using System.IO;
using System.Linq;
using System.Text;

namespace ICLibrary.ICCard
{
    public class Helper
    {

        private static UIntPtr m_hr;// 读卡器操作句柄
        private static UIntPtr m_ht;// 标签操作句柄
        private static string sUid;//UID
        private static bool isOpen;

        public static void ExtractAllResources(Stream st)
        {
            //释放Http, IC卡相关dll、driver
            using (var zip = ZipFile.Read(st))
            {
                zip.ExtractAll(".\\", ExtractExistingFileAction.DoNotOverwrite);
            }
        }


        /// <summary>
        /// Http服务，开始监听
        /// </summary>
        public static void Listen()
        {
            LoadDriver();
            isOpen = OpenDev();
            Console.WriteLine("Status: " + isOpen + "\r\n");
            Console.WriteLine("CardReader Service Listen Port: 33448\r\n\r\nUri: http://localhost:33448/tryread\r\nTest Uri: http://localhost:33448/");
            CeenHttpd.CeenHttpServer();
        }

        /// <summary>
        /// 获取响应内容
        /// </summary>
        /// <returns></returns>
        public static byte[] GetResponseText()
        {
            string cardNumber = TryRead();
            string responseString = "{\"code\":204}";
            if (!isOpen)
            {
                responseString = "{\"code\":500}";
            }
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length > 8)
            {
                Beep();
                responseString = "{\"code\":200,\"uid\":\"" + cardNumber.Substring(0, 8) + "\"}";
            }
            return Encoding.UTF8.GetBytes(responseString);
        }


        /// <summary>
        /// 加载驱动
        /// </summary>
        public static void LoadDriver()
        {
            const string drv_dir = ".\\Drivers";
            if (rfidlib_reader.RDR_LoadReaderDrivers(drv_dir) == 0)
            {
                for (uint i = 0; i < rfidlib_reader.RDR_GetLoadedReaderDriverCount(); i++)
                {
                    uint nSize = 0;
                    StringBuilder sb = new StringBuilder();
                    if (rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, rfidlib_def.LOADED_RDRDVR_OPT_CATALOG, sb, ref nSize) == 0)
                    {
                        string name = sb.ToString();
                        if (name.Equals(rfidlib_def.RDRDVR_TYPE_READER))
                        {
                            sb.Clear();
                            rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, rfidlib_def.LOADED_RDRDVR_OPT_NAME, sb, ref nSize);
                            Console.WriteLine(name + ": " + sb.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 与读卡器建立连接
        /// </summary>
        /// <returns>连接状态</returns>
        public static bool OpenDev()
        {
            const string connstr = "RDType=RL8000;CommType=USB;AddrMode=0";
            var hrOut = UIntPtr.Zero;
            bool isOpen = rfidlib_reader.RDR_Open(connstr, ref hrOut) == 0;
            if (isOpen)
            {
                m_hr = hrOut;
            }
            return isOpen;
        }

        /// <summary>
        /// 尝试读取卡片信息
        /// </summary>
        /// <returns>卡片信息</returns>
        public static string TryRead()
        {
            UIntPtr InvenParamSpecList = rfidlib_reader.RDR_CreateInvenParamSpecList();
            bool b_iso14443a = true;
            if (InvenParamSpecList != UIntPtr.Zero)
            {
                byte AntennaID = 0;
                rfidlib_aip_iso14443A.ISO14443A_CreateInvenParam(InvenParamSpecList, AntennaID);
            }
            byte newAI = rfidlib_def.AI_TYPE_NEW;
            for (int i = 0; i <= 5; i++)
            {
                byte[] AntennaIDs = new byte[64];
                if (rfidlib_reader.RDR_TagInventory(m_hr, newAI, 0, AntennaIDs, InvenParamSpecList) != 0)
                {
                    continue;
                }
                UIntPtr dnhReport = rfidlib_reader.RDR_GetTagDataReport(m_hr, rfidlib_def.RFID_SEEK_FIRST);
                while (dnhReport != UIntPtr.Zero)
                {
                    newAI = rfidlib_def.AI_TYPE_NEW;
                    if (b_iso14443a)
                    {
                        uint aip_id = 0;
                        uint tag_id = 0;
                        uint ant_id = 0;
                        byte[] uid = new byte[32];
                        byte uidlen = 0;
                        if (rfidlib_aip_iso14443A.ISO14443A_ParseTagDataReport(dnhReport, ref aip_id, ref tag_id, ref ant_id, uid,
                                ref uidlen) == 0)
                        {
                            string sUid = EncodeHexStr(uid);
                            //sUid
                            Helper.sUid = sUid;
                        }
                    }
                    dnhReport = rfidlib_reader.RDR_GetTagDataReport(m_hr, rfidlib_def.RFID_SEEK_NEXT);
                }
            }
            if (InvenParamSpecList != UIntPtr.Zero)
            {
                rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
            }
            string cardNumber = string.Empty;
            rfidlib_reader.RDR_ResetCommuImmeTimeout(m_hr);
            if (sUid != null)
            {
                byte[] uid = DecodeHex(sUid);
                UIntPtr ht = UIntPtr.Zero;
                int con_code = rfidlib_aip_iso14443A.MFCL_Connect(m_hr, 0, uid, ref ht);
                m_ht = ht;
                if (con_code == 0)
                {
                    byte[] key = DecodeHex("FFFFFFFFFFFF");
                    byte keyType = 0;
                    byte blkAddr = 0;
                    int auth_code = rfidlib_aip_iso14443A.MFCL_Authenticate(m_hr, m_ht, blkAddr, keyType, key);
                    if (auth_code == 0)
                    {
                        byte[] blkData = new byte[16];
                        int read_code = rfidlib_aip_iso14443A.MFCL_ReadBlock(m_hr, m_ht, blkAddr, blkData, 16);
                        if (read_code == 0)
                        {
                            cardNumber = EncodeHexStr(blkData);
                        }
                    }
                    rfidlib_reader.RDR_TagDisconnect(m_hr, m_ht);
                    m_ht = UIntPtr.Zero;
                }
            }
            return cardNumber;
        }

        /// <summary>
        /// 关闭句柄
        /// </summary>
        public static void Close()
        {
            rfidlib_reader.RDR_Close(m_hr);
            m_hr = UIntPtr.Zero;
        }

        /// <summary>
        /// 提示音(可选)
        /// </summary>
        public static void Beep()
        {
            byte activeDuration = (1) & 0xff;
            byte number = 0 + 1;
            byte pauseDuration = (1) & 0xff;
            UIntPtr dnOutputOper = rfidlib_reader.RDR_CreateSetOutputOperations();
            rfidlib_reader.RDR_AddOneOutputOperation(dnOutputOper, (0 + 1), 3, number, (uint)(activeDuration * 100), (uint)(pauseDuration * 100));
            rfidlib_reader.RDR_SetOutput(m_hr, dnOutputOper);
            rfidlib_reader.DNODE_Destroy(dnOutputOper);
        }

        private static byte[] DecodeHex(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                          .Where(x => x % 2 == 0)
                          .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                          .ToArray();
        }
        private static string EncodeHexStr(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
    }
}