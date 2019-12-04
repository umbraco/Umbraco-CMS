using System.Collections.Generic;

namespace Umbraco.Core.IO
{
    public interface IIOHelper
    {
        bool ForceNotHosted { get; set; }

        char DirSepChar { get; }
        string FindFile(string virtualPath);
        string ResolveVirtualUrl(string path);
        string ResolveUrl(string virtualPath);
        Attempt<string> TryResolveUrl(string virtualPath);
        string MapPath(string path, bool useHttpContext);
        string MapPath(string path);


        /// <summary>
        /// Verifies that the current filepath matches a directory where the user is allowed to edit a file.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validDir">The valid directory.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        bool VerifyEditPath(string filePath, string validDir);

        /// <summary>
        /// Verifies that the current filepath matches one of several directories where the user is allowed to edit a file.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validDirs">The valid directories.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        bool VerifyEditPath(string filePath, IEnumerable<string> validDirs);

        /// <summary>
        /// Verifies that the current filepath has one of several authorized extensions.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validFileExtensions">The valid extensions.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        bool VerifyFileExtension(string filePath, IEnumerable<string> validFileExtensions);

        bool PathStartsWith(string path, string root, char separator);

        /// <summary>
        /// Returns the path to the root of the application, by getting the path to where the assembly where this
        /// method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
        /// even if the assembly is in a /bin/debug or /bin/release folder
        /// </summary>
        /// <returns></returns>
        string GetRootDirectorySafe();

        string GetRootDirectoryBinFolder();

        /// <summary>
        /// Allows you to overwrite RootDirectory, which would otherwise be resolved
        /// automatically upon application start.
        /// </summary>
        /// <remarks>The supplied path should be the absolute path to the root of the umbraco site.</remarks>
        /// <param name="rootPath"></param>
        void SetRootDirectory(string rootPath);

        /// <summary>
        /// Check to see if filename passed has any special chars in it and strips them to create a safe filename.  Used to overcome an issue when Umbraco is used in IE in an intranet environment.
        /// </summary>
        /// <param name="filePath">The filename passed to the file handler from the upload field.</param>
        /// <returns>A safe filename without any path specific chars.</returns>
        string SafeFileName(string filePath);

        void EnsurePathExists(string path);

        /// <summary>
        /// Get properly formatted relative path from an existing absolute or relative path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetRelativePath(string path);

        /// <summary>
        /// Gets the root path of the application
        /// </summary>
        string Root
        {
            get;
            set; //Only required for unit tests
        }
    }
}
