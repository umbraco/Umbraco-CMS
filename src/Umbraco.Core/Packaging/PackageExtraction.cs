﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace Umbraco.Core.Packaging
{
    internal class PackageExtraction : IPackageExtraction
    {
        public string ReadTextFileFromArchive(string packageFilePath, string fileToRead, out string directoryInPackage)
        {
            string retVal = null;
            bool fileFound = false;
            string foundDir = null;

            ReadZipfileEntries(packageFilePath, entry =>
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

            string extension = Path.GetExtension(packageFilePath).ToLower();

            var alowedExtension = new[] { ".umb", ".zip" };

            // Check if the file is a valid package
            if (alowedExtension.All(ae => ae.Equals(extension) == false))
            {
                throw new ArgumentException(
                    string.Format("Error - file isn't a package. only extentions: \"{0}\" is allowed", string.Join(", ", alowedExtension)));
            }
        }
        
        public void CopyFileFromArchive(string packageFilePath, string fileInPackageName, string destinationfilePath)
        {
            CopyFilesFromArchive(packageFilePath, new[]{new KeyValuePair<string, string>(fileInPackageName, destinationfilePath) } );
        }

        public void CopyFilesFromArchive(string packageFilePath, IEnumerable<KeyValuePair<string, string>> sourceDestination)
        {
            var d = sourceDestination.ToDictionary(k => k.Key.ToLower(), v => v.Value);


            ReadZipfileEntries(packageFilePath, entry =>
            {
                string fileName = (Path.GetFileName(entry.Name) ?? string.Empty).ToLower();
                if (fileName == string.Empty) { return true; }

                string destination;
                if (string.IsNullOrEmpty(fileName) == false && d.TryGetValue(fileName, out destination))
                {
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
                throw new ArgumentException(string.Format("The following source file(s): \"{0}\" could not be found in archive: \"{1}\"", string.Join("\", \"",d.Keys), packageFilePath));
            }
        }

        public IEnumerable<string> FindMissingFiles(string packageFilePath, IEnumerable<string> expectedFiles)
        {
            var retVal = expectedFiles.ToList();

            ReadZipfileEntries(packageFilePath, zipEntry =>
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


            ReadZipfileEntries(packageFilePath, entry =>
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

        public IEnumerable<byte[]> ReadFilesFromArchive(string packageFilePath, IEnumerable<string> filesToGet)
        {
            CheckPackageExists(packageFilePath);

            var files = new HashSet<string>(filesToGet.Select(f => f.ToLowerInvariant()));

            using (var fs = File.OpenRead(packageFilePath))
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

        private void ReadZipfileEntries(string packageFilePath, Func<ZipArchiveEntry, bool> entryFunc, bool skipsDirectories = true)
        {
            CheckPackageExists(packageFilePath);

            using (var fs = File.OpenRead(packageFilePath))
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