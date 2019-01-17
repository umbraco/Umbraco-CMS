using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace Umbraco.Core.Packaging
{
    internal class PackageExtraction
    {
        public string ReadTextFileFromArchive(FileInfo packageFile, string fileToRead, out string directoryInPackage)
        {
            string retVal = null;
            bool fileFound = false;
            string foundDir = null;

            ReadZipfileEntries(packageFile, entry =>
            {
                string fileName = Path.GetFileName(entry.Name);

                if (string.IsNullOrEmpty(fileName) == false && fileName.Equals(fileToRead, StringComparison.CurrentCultureIgnoreCase))
                {

                    foundDir = entry.Name.Substring(0, entry.Name.Length - fileName.Length);
                    fileFound = true;
                    using (var entryStream = entry.Open())
                    using (var reader = new StreamReader(entryStream))
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
                throw new FileNotFoundException($"Could not find file in package {packageFile}", fileToRead);
            }
            directoryInPackage = foundDir;
            return retVal;
        }

        private static void CheckPackageExists(FileInfo packageFile)
        {
            if (packageFile == null) throw new ArgumentNullException(nameof(packageFile));
            
            if (!packageFile.Exists)
                throw new ArgumentException($"Package file: {packageFile} could not be found");

            var extension = packageFile.Extension;

            var alowedExtension = new[] { ".umb", ".zip" };

            // Check if the file is a valid package
            if (alowedExtension.All(ae => ae.InvariantEquals(extension) == false))
            {
                throw new ArgumentException("Error - file isn't a package. only extentions: \"{string.Join(", ", alowedExtension)}\" is allowed");
            }
        }

        public void CopyFileFromArchive(FileInfo packageFile, string fileInPackageName, string destinationfilePath)
        {
            CopyFilesFromArchive(packageFile, new[] {(fileInPackageName, destinationfilePath)});
        }

        public void CopyFilesFromArchive(FileInfo packageFile, IEnumerable<(string packageUniqueFile, string appAbsolutePath)> sourceDestination)
        {
            var d = sourceDestination.ToDictionary(k => k.packageUniqueFile.ToLower(), v => v.appAbsolutePath);


            ReadZipfileEntries(packageFile, entry =>
            {
                var fileName = (Path.GetFileName(entry.Name) ?? string.Empty).ToLower();
                if (fileName == string.Empty) { return true; }

                if (string.IsNullOrEmpty(fileName) == false && d.TryGetValue(fileName, out var destination))
                {
                    //ensure the dir exists
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));

                    using (var streamWriter = File.Open(destination, FileMode.Create))
                    using (var entryStream = entry.Open())
                    {
                        entryStream.CopyTo(streamWriter);
                    }

                    d.Remove(fileName);
                    return d.Any();
                }
                return true;
            });

            if (d.Any())
            {
                throw new ArgumentException(string.Format("The following source file(s): \"{0}\" could not be found in archive: \"{1}\"", string.Join("\", \"",d.Keys), packageFile));
            }
        }

        public IEnumerable<string> FindMissingFiles(FileInfo packageFile, IEnumerable<string> expectedFiles)
        {
            var retVal = expectedFiles.ToList();

            ReadZipfileEntries(packageFile, zipEntry =>
            {
                string fileName = Path.GetFileName(zipEntry.Name);

                int index = retVal.FindIndex(f => f.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));

                if (index != -1) { retVal.RemoveAt(index); }

                return retVal.Any();
            });
            return retVal;

        }

        public IEnumerable<string> FindDuplicateFileNames(FileInfo packageFile)
        {
            var dictionary = new Dictionary<string, List<string>>();


            ReadZipfileEntries(packageFile, entry =>
            {
                var fileName = (Path.GetFileName(entry.Name) ?? string.Empty).ToLower();

                if (dictionary.TryGetValue(fileName, out var list) == false)
                {
                    list = new List<string>();
                    dictionary.Add(fileName, list);
                }

                list.Add(entry.Name);

                return true;
            });

            return dictionary.Values.Where(v => v.Count > 1).SelectMany(v => v);
        }

        public IEnumerable<byte[]> ReadFilesFromArchive(FileInfo packageFile, IEnumerable<string> filesToGet)
        {
            CheckPackageExists(packageFile);

            var files = new HashSet<string>(filesToGet.Select(f => f.ToLowerInvariant()));

            using (var fs = packageFile.OpenRead())
            using (var zipArchive = new ZipArchive(fs))
            {
                foreach (var zipEntry in zipArchive.Entries)
                {
                    if (zipEntry.Name.IsNullOrWhiteSpace() && zipEntry.FullName.EndsWith("/")) continue;

                    if (files.Contains(zipEntry.Name.ToLowerInvariant()))
                    {
                        using (var memStream = new MemoryStream())
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.CopyTo(memStream);
                            memStream.Close();
                            yield return memStream.ToArray();
                        }
                    }
                }
            }
        }

        private void ReadZipfileEntries(FileInfo packageFile, Func<ZipArchiveEntry, bool> entryFunc, bool skipsDirectories = true)
        {
            CheckPackageExists(packageFile);

            using (var fs = packageFile.OpenRead())
            using (var zipArchive = new ZipArchive(fs))
            {
                foreach (var zipEntry in zipArchive.Entries)
                {
                    if (zipEntry.Name.IsNullOrWhiteSpace() && zipEntry.FullName.EndsWith("/") && skipsDirectories)
                        continue;
                    if (entryFunc(zipEntry) == false) break;
                }
            }
        }
    }
}
