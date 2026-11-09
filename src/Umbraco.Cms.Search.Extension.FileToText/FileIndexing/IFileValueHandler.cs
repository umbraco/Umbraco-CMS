namespace Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

/// <summary>
/// Extracts the text contents of a file, so it can be indexed for search.
/// </summary>
/// <remarks>
/// Implementations are registered in the service collection as <see cref="IFileValueHandler"/>. When more than one handler
/// can handle a file extension, a custom handler takes precedence over the built-in ones.
/// </remarks>
public interface IFileValueHandler
{
    /// <summary>
    /// Determines whether the handler can extract text from files with the given extension.
    /// </summary>
    /// <param name="extension">The file extension, including the leading dot (for example, <c>.pdf</c>). Casing is not normalized.</param>
    /// <returns>True if the handler can handle the file extension, false otherwise.</returns>
    bool CanHandle(string extension);

    /// <summary>
    /// Extracts the text contents of a file.
    /// </summary>
    /// <param name="stream">The file contents. The caller owns the stream and disposes it afterwards.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The text contents of the file. An empty or whitespace result means the file contributes nothing to the index.</returns>
    Task<string> GetFileContentsAsync(Stream stream, CancellationToken cancellationToken);
}
