namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents a deployable file type.
/// </summary>
public interface IFileType
{
    /// <summary>
    /// Gets a value indicating whether the file can be set using a physical path.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the file can be set using a physical path; otherwise, <c>false</c>.
    /// </value>
    [Obsolete("An interface should not expose implementation details. Scheduled for removal in Umbraco 18.")]
    bool CanSetPhysical { get; }

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
    /// Gets the checksum stream.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The checksum stream.
    /// </returns>
    [Obsolete("Use GetChecksumStreamAsync() instead. Scheduled for removal in Umbraco 18.")]
    Stream GetChecksumStream(StringUdi udi);

    /// <summary>
    /// Gets the checksum stream in an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation. The task result contains the checksum stream.
    /// </returns>
    Task<Stream> GetChecksumStreamAsync(StringUdi udi, CancellationToken cancellationToken = default)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(GetChecksumStream(udi));
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets the file length in bytes or <c>-1</c> if not found.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The file length in bytes or <c>-1</c> if not found.
    /// </returns>
    [Obsolete("Use GetLengthAsync() instead. Scheduled for removal in Umbraco 18.")]
    long GetLength(StringUdi udi);

    /// <summary>
    /// Gets the file length in bytes or <c>-1</c> if not found in an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation. The task result contains the file length in bytes or <c>-1</c> if not found.
    /// </returns>
    Task<long> GetLengthAsync(StringUdi udi, CancellationToken cancellationToken = default)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(GetLength(udi));
#pragma warning restore CS0618 // Type or member is obsolete

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
    /// Sets the physical path of the file.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="physicalPath">The physical path.</param>
    /// <param name="copy">If set to <c>true</c> copies the file instead of moving.</param>
    [Obsolete("Use SetStreamAsync() instead to not rely on physical file paths. Scheduled for removal in Umbraco 18.")]
    void Set(StringUdi udi, string physicalPath, bool copy = false);

    /// <summary>
    /// Gets the path to the file, including the file name. Returns <see cref="string.Empty" /> if the file is not directly accessible.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The path to the file, including the file name or <see cref="string.Empty" /> if the file is not directly accessible.
    /// </returns>
    string GetPhysicalPath(StringUdi udi);

    /// <summary>
    /// Gets the virtual path or <see cref="string.Empty"/> if not found.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The virtual path or <see cref="string.Empty"/> if not found.
    /// </returns>
    [Obsolete("This is not used anymore. Scheduled for removal in Umbraco 18.")]
    string GetVirtualPath(StringUdi udi);
}
