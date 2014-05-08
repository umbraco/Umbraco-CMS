using System.Collections.Generic;

namespace Umbraco.Core.Packaging
{
    public interface IUnpackHelper
    {
        string ReadTextFileFromArchive(string packageFilePath, string fileToRead);
        bool CopyFileFromArchive(string packageFilePath, string fileInPackageName, string destinationfilePath);
        IEnumerable<string> FindMissingFiles(string packageFilePath, IEnumerable<string> expectedFiles);
    }
}