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
                    ZipEntry theEntry;
                    while ((theEntry = zipStream.GetNextEntry()) != null)
                    {
                        if (theEntry.Name.EndsWith(fileToRead, StringComparison.CurrentCultureIgnoreCase))
                        {
                            using (var reader = new StreamReader(zipStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    };
                }
            }

            throw new FileNotFoundException("Could not find file in package file " + sourcefilePath, fileToRead);
        }

        public void UnPack(string sourcefilePath, string destinationDirectory)
        {
            // Unzip
            using (var fs = File.OpenRead(sourcefilePath))
            {
                using (var s = new ZipInputStream(fs))
                {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (fileName == String.Empty) continue;

                        using ( var streamWriter = File.Create(destinationDirectory + Path.DirectorySeparatorChar + fileName)) 
                        {
                            var data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}