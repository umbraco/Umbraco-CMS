using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace Umbraco.Core.Packaging
{
    public class UnpackHelper : IUnpackHelper
    {
        public string ReadTextFileFromArchive(string packageFilePath, string fileToRead, out string directoryInPackage)
        {

            string retVal = null;
            bool fileFound = false;
            string foundDir = null;

            ReadZipfileEntries(packageFilePath, (entry, stream) =>
            {
                string fileName = Path.GetFileName(entry.Name);

                if (string.IsNullOrEmpty(fileName) == false &&
                    fileName.Equals(fileToRead, StringComparison.CurrentCultureIgnoreCase))
                {

                    foundDir = entry.Name.Substring(0, entry.Name.Length - fileName.Length);
                    fileFound = true;
                    using (var reader = new StreamReader(stream))
                    {
                        retVal = reader.ReadToEnd();
                        return false;
                    }
                }
                return true;
            });

            if (fileFound == false)
            {
                directoryInPackage = null;
                throw new FileNotFoundException(string.Format("Could not find file in package {0}", packageFilePath), fileToRead);
            }
            directoryInPackage = foundDir;
            return retVal;            
        }

        private static void CheckPackageExists(string packageFilePath)
        {
            if (string.IsNullOrEmpty(packageFilePath))
            {
                throw new ArgumentNullException("packageFilePath");
            }

            if (File.Exists(packageFilePath) == false)
            {
                if (File.Exists(packageFilePath) == false)
                    throw new ArgumentException(string.Format("Package file: {0} could not be found", packageFilePath));
            }

            // Check if the file is a valid package
            if (Path.GetExtension(packageFilePath).Equals(".umb", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                throw new ArgumentException(
                    "Error - file isn't a package (doesn't have a .umb extension). Check if the file automatically got named '.zip' upon download.");
            }
        }


        public bool CopyFileFromArchive(string packageFilePath, string fileInPackageName, string destinationfilePath)
        {
            bool fileFoundInArchive = false;
            bool fileOverwritten = false;

            ReadZipfileEntries(packageFilePath, (entry, stream) =>
            {
                string fileName = Path.GetFileName(entry.Name);

                if (string.IsNullOrEmpty(fileName) == false &&
                    fileName.Equals(fileInPackageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    fileFoundInArchive = true;

                    fileOverwritten = File.Exists(destinationfilePath);

                    using (var streamWriter = File.Open(destinationfilePath, FileMode.Create))
                    {
                        var data = new byte[2048];
                        int size;
                        while ((size = stream.Read(data, 0, data.Length)) > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }

                        streamWriter.Close();
                    }
                    return false;
                }
                return true;
            });


            if (fileFoundInArchive == false) throw new ArgumentException(string.Format("Could not find file: {0} in package file: {1}", fileInPackageName, packageFilePath), "fileInPackageName");

            return fileOverwritten;
        }

        public IEnumerable<string> FindMissingFiles(string packageFilePath, IEnumerable<string> expectedFiles)
        {
            var retVal = expectedFiles.ToList();

            ReadZipfileEntries(packageFilePath, (zipEntry, stream) =>
            {
                string fileName = Path.GetFileName(zipEntry.Name);

                int index = retVal.FindIndex(f => f.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));

                if (index != -1) { retVal.RemoveAt(index); }

                return retVal.Any();
            });
            return retVal;

        }

        public IEnumerable<string> FindDubletFileNames(string packageFilePath)
        {
            var dictionary = new Dictionary<string, List<string>>();


            ReadZipfileEntries(packageFilePath, (entry, stream) =>
            {
                string fileName = (Path.GetFileName(entry.Name) ?? string.Empty).ToLower();

                List<string> list;
                if (dictionary.TryGetValue(fileName, out list) == false)
                {
                    list = new List<string>();
                    dictionary.Add(fileName, list);
                }

                list.Add(entry.Name);
                
                return true;
            });

            return dictionary.Values.Where(v => v.Count > 1).SelectMany(v => v);
        }

        private void ReadZipfileEntries(string packageFilePath, Func<ZipEntry, ZipInputStream, bool> entryFunc, bool skipsDirectories = true)
        {
            CheckPackageExists(packageFilePath);

            using (var fs = File.OpenRead(packageFilePath))
            {
                using (var zipInputStream = new ZipInputStream(fs))
                {
                    ZipEntry zipEntry;
                    while ((zipEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (zipEntry.IsDirectory && skipsDirectories) continue;
                        if( entryFunc(zipEntry, zipInputStream) == false ) break;
                    }

                    zipInputStream.Close();
                }
                fs.Close();
            }
        }
    }
}