using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core.IO;

namespace Umbraco.Core.Packaging
{
    public class UnpackHelper : IUnpackHelper
    {
        public string UnPackToTempDirectory(string filePath)
        {
            string tempDir = IOHelper.MapPath(SystemDirectories.Data) + Path.DirectorySeparatorChar + Guid.NewGuid();
            Directory.CreateDirectory(tempDir);
            UnPack(filePath, tempDir);
            return tempDir;
        }

        public string ReadTextFileFromArchive(string sourcefilePath, string fileToRead)
        {
            using (var fs = File.OpenRead(sourcefilePath))
            {
                using (var zipStream = new ZipInputStream(fs))
                {
                    ZipEntry zipEntry;
                    while ((zipEntry = zipStream.GetNextEntry()) != null)
                    {
                        if (zipEntry.Name.EndsWith(fileToRead, StringComparison.CurrentCultureIgnoreCase))
                        {
                            using (var reader = new StreamReader(zipStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }

                    zipStream.Close();
                }
                fs.Close();
            }

            throw new FileNotFoundException(string.Format("Could not find file in package file {0}", sourcefilePath), fileToRead);
        }

        public void UnPack(string sourcefilePath, string destinationDirectory)
        {
            // Unzip
            using (var fs = File.OpenRead(sourcefilePath))
            {
                using (var zipInputStream = new ZipInputStream(fs))
                {
                    ZipEntry zipEntry;
                    while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        string fileName = Path.GetFileName(zipEntry.Name);
                        if (string.IsNullOrEmpty(fileName)) continue;

                        using ( var streamWriter = File.Create(Path.Combine(destinationDirectory, fileName))) 
                        {
                            var data = new byte[2048];
                            int size;
                            while ((size = zipInputStream.Read(data, 0, data.Length)) > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }

                            streamWriter.Close();
                        }
                    }

                    zipInputStream.Close();
                }
                fs.Close();
            }
        }
    }
}