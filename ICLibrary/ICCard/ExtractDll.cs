using System.IO;
using System.Linq;
using System.Reflection;

namespace ICLibrary.ICCard
{
    public class ExtractDll
    {
        /// <summary>
        /// 释放依赖库
        /// </summary>
        public static void Depends()
        {
            var files = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList();
            var ZipLib = files.First(name => name.Contains("DotNetZip.dll"));
            var DependsLib = files.First(name => name.Contains("ic.zip"));
            using (var fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(ZipLib))
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(fs))
                {
                    fileData = binaryReader.ReadBytes((int)fs.Length);
                    File.WriteAllBytes(".\\DotNetZip.dll", fileData);
                }
            }
            Helper.ExtractAllResources(Assembly.GetExecutingAssembly().GetManifestResourceStream(DependsLib));
        }
    }
}
