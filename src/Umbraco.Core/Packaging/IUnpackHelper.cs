using System.Collections.Generic;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Used to access an umbraco package file
    /// Remeber that filenames must be unique
    /// use "FindDubletFileNames" for sanitycheck
    /// </summary>
    public interface IUnpackHelper
    {
        /// <summary>
        /// Returns the content of the file with the given filename
        /// </summary>
        /// <param name="packageFilePath">Full path to the ubraco package file</param>
        /// <param name="fileToRead">filename of the file for wich to get the text content</param>
        /// <param name="directoryInPackage">this is the relative directory for the location of the file in the package
        /// I dont know why umbraco packages contains directories in the first place??</param>
        /// <returns>text content of the file</returns>
        string ReadTextFileFromArchive(string packageFilePath, string fileToRead, out string directoryInPackage);

        /// <summary>
        /// Copies a file from package to given destination
        /// </summary>
        /// <param name="packageFilePath">Full path to the ubraco package file</param>
        /// <param name="fileInPackageName">filename of the file to copy</param>
        /// <param name="destinationfilePath">destination path (including destination filename)</param>
        /// <returns>True a file was overwritten</returns>
        bool CopyFileFromArchive(string packageFilePath, string fileInPackageName, string destinationfilePath);

        /// <summary>
        /// Check if given list of files can be found in the package
        /// </summary>
        /// <param name="packageFilePath">Full path to the ubraco package file</param>
        /// <param name="expectedFiles">a list of files you would like to find in the package</param>
        /// <returns>a subset if any of the files in "expectedFiles" that could not be found in the package</returns>
        IEnumerable<string> FindMissingFiles(string packageFilePath, IEnumerable<string> expectedFiles);


        /// <summary>
        /// Sanitycheck - should return en empty collection if package is valid
        /// </summary>
        /// <param name="packageFilePath">Full path to the ubraco package file</param>
        /// <returns>list of files that can are found more than ones (accross directories) in the package</returns>
        IEnumerable<string> FindDubletFileNames(string packageFilePath);

    }
}