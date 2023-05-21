namespace Umbraco.Cms.Core.IO;

public interface IIOHelper
{
    string FindFile(string virtualPath);

    [Obsolete("Use IHostingEnvironment.ToAbsolute instead")]
    string ResolveUrl(string virtualPath);

    /// <summary>
    ///     Maps a virtual path to a physical path in the content root folder (i.e. www)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [Obsolete("Use IHostingEnvironment.MapPathContentRoot or IHostingEnvironment.MapPathWebRoot instead")]
    string MapPath(string path);

    /// <summary>
    ///     Verifies that the current filepath matches a directory where the user is allowed to edit a file.
    /// </summary>
    /// <param name="filePath">The filepath to validate.</param>
    /// <param name="validDir">The valid directory.</param>
    /// <returns>A value indicating whether the filepath is valid.</returns>
    bool VerifyEditPath(string filePath, string validDir);

    /// <summary>
    ///     Verifies that the current filepath matches one of several directories where the user is allowed to edit a file.
    /// </summary>
    /// <param name="filePath">The filepath to validate.</param>
    /// <param name="validDirs">The valid directories.</param>
    /// <returns>A value indicating whether the filepath is valid.</returns>
    bool VerifyEditPath(string filePath, IEnumerable<string> validDirs);

    /// <summary>
    ///     Verifies that the current filepath has one of several authorized extensions.
    /// </summary>
    /// <param name="filePath">The filepath to validate.</param>
    /// <param name="validFileExtensions">The valid extensions.</param>
    /// <returns>A value indicating whether the filepath is valid.</returns>
    bool VerifyFileExtension(string filePath, IEnumerable<string> validFileExtensions);

    bool PathStartsWith(string path, string root, params char[] separators);

    void EnsurePathExists(string path);

    /// <summary>
    ///     Get properly formatted relative path from an existing absolute or relative path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string GetRelativePath(string path);

    /// <summary>
    ///     Retrieves array of temporary folders from the hosting environment.
    /// </summary>
    /// <returns>Array of <see cref="DirectoryInfo" /> instances.</returns>
    DirectoryInfo[] GetTempFolders();

    /// <summary>
    ///     Cleans contents of a folder by deleting all files older that the provided age.
    ///     If deletition of any file errors (e.g. due to a file lock) the process will continue to try to delete all that it
    ///     can.
    /// </summary>
    /// <param name="folder">Folder to clean.</param>
    /// <param name="age">Age of files within folder to delete.</param>
    /// <returns>Result of operation</returns>
    CleanFolderResult CleanFolder(DirectoryInfo folder, TimeSpan age);
}
