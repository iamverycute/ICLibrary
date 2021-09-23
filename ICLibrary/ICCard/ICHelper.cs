using System.IO;
using System.Linq;
using System.Reflection;

namespace ICLibrary.ICCard
{
    public class ICHelper : rfidlib_helper
    {
        private readonly static ICHelper ext_dll = new ICHelper();
        /// <summary>
        /// 释放依赖库
        /// </summary>
        public static ICHelper Depends()
        {
            string ZipLibPath = ".\\DotNetZip.dll";
            var asm = Assembly.GetExecutingAssembly();
            var files = asm.GetManifestResourceNames();
            var ZipLib = files.First(name => name.Contains("DotNetZip.dll"));
            var DependsLib = files.First(name => name.Contains("ic.zip"));
            if (!File.Exists(ZipLibPath))
            {
                using (var fs = asm.GetManifestResourceStream(ZipLib))
                {
                    using (var fw = File.OpenWrite(ZipLibPath))
                    {
                        fs.CopyTo(fw);
                    }
                }
                ExtractAllResources(asm.GetManifestResourceStream(DependsLib));
            }
            return ext_dll;
        }

        /// <summary>
        /// HTTP监听
        /// </summary>
        public void ListenA()
        {
            Listen();
        }
    }
}