namespace Umbraco.Cms.Core.IO;

/// <summary>
///     Provides methods allowing the manipulation of files.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    ///     Gets a value indicating whether the filesystem can add/copy
    ///     a file which is on a physical filesystem.
    /// </summary>
    /// <remarks>
    ///     In other words, whether the filesystem can copy/move a file
    ///     that is on local disk, in a fast and efficient way.
    /// </remarks>
    bool CanAddPhysical { get; }

    /// <summary>
    ///     Gets all directories matching the given path.
    /// </summary>
    /// <param name="path">The path to the directories.</param>
    /// <returns>
    ///     The <see cref="IEnumerable{String}" /> representing the matched directories.
    /// </returns>
    IEnumerable<string> GetDirectories(string path);

    /// <summary>
    ///     Deletes the specified directory.
    /// </summary>
    /// <param name="path">The name of the directory to remove.</param>
    void DeleteDirectory(string path);

    /// <summary>
    ///     Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
    /// </summary>
    /// <remarks>Azure blob storage has no real concept of directories so deletion is always recursive.</remarks>
    /// <param name="path">The name of the directory to remove.</param>
    /// <param name="recursive">Whether to remove directories, subdirectories, and files in path.</param>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    ///     Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The directory to check.</param>
    /// <returns>
    ///     <c>True</c> if the directory exists and the user has permission to view it; otherwise <c>false</c>.
    /// </returns>
    bool DirectoryExists(string path);

    /// <summary>
    ///     Adds a file to the file system.
    /// </summary>
    /// <param name="path">The path to the given file.</param>
    /// <param name="stream">The <see cref="Stream" /> containing the file contents.</param>
    void AddFile(string path, Stream stream);

    /// <summary>
    ///     Adds a file to the file system.
    /// </summary>
    /// <param name="path">The path to the given file.</param>
    /// <param name="stream">The <see cref="Stream" /> containing the file contents.</param>
    /// <param name="overrideIfExists">Whether to override the file if it already exists.</param>
    void AddFile(string path, Stream stream, bool overrideIfExists);

    /// <summary>
    ///     Gets all files matching the given path.
    /// </summary>
    /// <param name="path">The path to the files.</param>
    /// <returns>
    ///     The <see cref="IEnumerable{String}" /> representing the matched files.
    /// </returns>
    IEnumerable<string> GetFiles(string path);

    /// <summary>
    ///     Gets all files matching the given path and filter.
    /// </summary>
    /// <param name="path">The path to the files.</param>
    /// <param name="filter">A filter that allows the querying of file extension.
    ///     <example>*.jpg</example>
    /// </param>
    /// <returns>
    ///     The <see cref="IEnumerable{String}" /> representing the matched files.
    /// </returns>
    IEnumerable<string> GetFiles(string path, string filter);

    /// <summary>
    ///     Gets a <see cref="Stream" /> representing the file at the given path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>
    ///     <see cref="Stream" />.
    /// </returns>
    Stream OpenFile(string path);

    /// <summary>
    ///     Deletes the specified file.
    /// </summary>
    /// <param name="path">The name of the file to remove.</param>
    void DeleteFile(string path);

    /// <summary>
    ///     Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns>
    ///     <c>True</c> if the file exists and the user has permission to view it; otherwise <c>false</c>.
    /// </returns>
    bool FileExists(string path);

    /// <summary>
    ///     Returns the application relative path to the file.
    /// </summary>
    /// <param name="fullPathOrUrl">The full path or URL.</param>
    /// <returns>
    ///     The <see cref="string" /> representing the relative path.
    /// </returns>
    string GetRelativePath(string fullPathOrUrl);

    /// <summary>
    ///     Gets the full qualified path to the file.
    /// </summary>
    /// <param name="path">The file to return the full path for.</param>
    /// <returns>
    ///     The <see cref="string" /> representing the full path.
    /// </returns>
    string GetFullPath(string path);

    /// <summary>
    ///     Returns the application relative URL to the file.
    /// </summary>
    /// <param name="path">The path to return the URL for.</param>
    /// <returns>
    ///     <see cref="string" /> representing the relative URL.
    /// </returns>
    string GetUrl(string? path);

    /// <summary>
    ///     Gets the last modified date/time of the file, expressed as a UTC value.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>
    ///     <see cref="DateTimeOffset" />.
    /// </returns>
    DateTimeOffset GetLastModified(string path);

    /// <summary>
    ///     Gets the created date/time of the file, expressed as a UTC value.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>
    ///     <see cref="DateTimeOffset" />.
    /// </returns>
    DateTimeOffset GetCreated(string path);

    /// <summary>
    ///     Gets the size of a file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The size (in bytes) of the file.</returns>
    long GetSize(string path);

    /// <summary>
    ///     Adds a file which is on a physical filesystem.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="physicalPath">The absolute physical path to the source file.</param>
    /// <param name="overrideIfExists">A value indicating what to do if the file already exists.</param>
    /// <param name="copy">A value indicating whether to move (default) or copy.</param>
    void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false);

    // TODO: implement these
    //
    // void CreateDirectory(string path);
    //
    //// move or rename, directory or file
    // void Move(string source, string target);
}
