namespace Umbraco.Cms.Core.Services;

public interface IBasicFileService
{
    /// <summary>
    /// Get the content of a file as a stream.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A stream containing the contents of the file.</returns>
    Task<Stream> GetContentStreamAsync(string path);

    /// <summary>
    /// Set the content of a file from a stream.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The desired content of the file as a stream.</param>
    Task SetContentStreamAsync(string path, Stream content);

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<long> GetFileSizeAsync(string path);
}
