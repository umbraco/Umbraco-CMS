using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using umbraco;

namespace umbraco.cms.businesslogic.utilities
{
    public class Zip
    {
        public static void UnPack(string ZipFilePath, string UnPackDirectory, bool DeleteZipFile)
        {
            ZipFile.ExtractToDirectory(ZipFilePath, UnPackDirectory);

            if (DeleteZipFile)
                File.Delete(ZipFilePath);
        }

    }
}
