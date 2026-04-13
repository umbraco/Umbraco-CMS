namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a deployable file type.
/// </summary>
public interface IFileType
{
    /// <summary>
    /// Gets the stream in an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation. The task result contains the stream.
    /// </returns>
    Task<Stream> GetStreamAsync(StringUdi udi, CancellationToken token); // TODO: Rename token to cancellationToken and add default value

    /// <summary>
    /// Gets the checksum stream in an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation. The task result contains the checksum stream.
    /// </returns>
    Task<Stream> GetChecksumStreamAsync(StringUdi udi, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file length in bytes or <c>-1</c> if not found in an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation. The task result contains the file length in bytes or <c>-1</c> if not found.
    /// </returns>
    Task<long> GetLengthAsync(StringUdi udi, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the stream as an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="stream">The stream.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation.
    /// </returns>
    Task SetStreamAsync(StringUdi udi, Stream stream, CancellationToken token); // TODO: Rename token to cancellationToken and add default value

    /// <summary>
    /// Gets the path to the file, including the file name. Returns <see cref="string.Empty" /> if the file is not directly accessible.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The path to the file, including the file name or <see cref="string.Empty" /> if the file is not directly accessible.
    /// </returns>
    string GetPhysicalPath(StringUdi udi);
}
