namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for file operations with folder support.
/// </summary>
public interface IFileWithFoldersRepository
{
    /// <summary>
    ///     Adds a new folder.
    /// </summary>
    /// <param name="folderPath">The path of the folder to add.</param>
    void AddFolder(string folderPath);

    /// <summary>
    ///     Deletes a folder.
    /// </summary>
    /// <param name="folderPath">The path of the folder to delete.</param>
    void DeleteFolder(string folderPath);

    /// <summary>
    ///     Checks whether a folder exists.
    /// </summary>
    /// <param name="folderPath">The path of the folder.</param>
    /// <returns><c>true</c> if the folder exists; otherwise, <c>false</c>.</returns>
    bool FolderExists(string folderPath);

    /// <summary>
    ///     Checks whether a folder contains any content.
    /// </summary>
    /// <param name="folderPath">The path of the folder.</param>
    /// <returns><c>true</c> if the folder contains content; otherwise, <c>false</c>.</returns>
    bool FolderHasContent(string folderPath);
}
