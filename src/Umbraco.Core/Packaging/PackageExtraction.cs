using System;
using System.IO;
using Umbraco.Core.IO;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageExtraction
    {
        bool Extract(string packageFilePath, string destinationFolder);
        string ExtractToTemporaryFolder(string packageFilePath);
        string GetPackageConfigFromArchive(string packageFilePath, string fileToRead = "package.xml");
    }

    internal class PackageExtraction : IPackageExtraction
    {
        public bool Extract(string packageFilePath, string destinationFolder)
        {
            return true;
        }

        public string ExtractToTemporaryFolder(string packageFilePath)
        {
            string tempDir = Path.Combine(IOHelper.MapPath(SystemDirectories.Data), Guid.NewGuid().ToString("D"));
            Directory.CreateDirectory(tempDir);
            Extract(packageFilePath, tempDir);
            return tempDir;
        }

        public string GetPackageConfigFromArchive(string packageFilePath, string fileToRead = "package.xml")
        {
            return string.Empty;
        }
    }
}