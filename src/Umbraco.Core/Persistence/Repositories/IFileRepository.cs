namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for file operations.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    ///     Gets a stream for reading the content of a file.
    /// </summary>
    /// <param name="filepath">The path to the file.</param>
    /// <returns>A stream for reading the file content.</returns>
    Stream GetFileContentStream(string filepath);

    /// <summary>
    ///     Sets the content of a file.
    /// </summary>
    /// <param name="filepath">The path to the file.</param>
    /// <param name="content">The stream containing the content to write.</param>
    void SetFileContent(string filepath, Stream content);

    /// <summary>
    ///     Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filepath">The path to the file.</param>
    /// <returns>The size of the file in bytes.</returns>
    long GetFileSize(string filepath);
}
