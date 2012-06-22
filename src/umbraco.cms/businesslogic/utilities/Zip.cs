using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using umbraco;

namespace umbraco.cms.businesslogic.utilities
{
    public class Zip
    {
        public static void UnPack(string ZipFilePath, string UnPackDirectory, bool DeleteZipFile)
        {
            // Unzip
            string tempDir = UnPackDirectory;
            Directory.CreateDirectory(tempDir);

            ZipInputStream s = new ZipInputStream(File.OpenRead(ZipFilePath));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(tempDir + Path.DirectorySeparatorChar + fileName);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();

                }
            }

            // Clean up
            s.Close();
            if (DeleteZipFile)
                File.Delete(ZipFilePath);

        }

    }
}
