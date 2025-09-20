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
    bool CanSetPhysical { get; }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The stream.
    /// </returns>
    Stream GetStream(StringUdi udi);

    /// <summary>
    /// Gets the stream as an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation.
    /// </returns>
    Task<Stream> GetStreamAsync(StringUdi udi, CancellationToken token);

    /// <summary>
    /// Gets the checksum stream.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The checksum stream.
    /// </returns>
    Stream GetChecksumStream(StringUdi udi);

    /// <summary>
    /// Gets the file length in bytes or <c>-1</c> if not found.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The file length in bytes or <c>-1</c> if not found.
    /// </returns>
    long GetLength(StringUdi udi);

    /// <summary>
    /// Sets the stream.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="stream">The stream.</param>
    void SetStream(StringUdi udi, Stream stream);

    /// <summary>
    /// Sets the stream as an asynchronous operation.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="stream">The stream.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>
    /// The task object representing the asynchronous operation.
    /// </returns>
    Task SetStreamAsync(StringUdi udi, Stream stream, CancellationToken token);

    /// <summary>
    /// Sets the physical path of the file.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <param name="physicalPath">The physical path.</param>
    /// <param name="copy">If set to <c>true</c> copies the file instead of moving.</param>
    void Set(StringUdi udi, string physicalPath, bool copy = false);

    /// <summary>
    /// Gets the physical path or <see cref="string.Empty"/> if not found.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The physical path or <see cref="string.Empty"/> if not found.
    /// </returns>
    /// <remarks>
    /// This is not pretty as *everywhere* in Deploy we take care of ignoring
    /// the physical path and always rely on the virtual IFileSystem,
    /// but Cloud wants to add some of these files to Git and needs the path...
    /// </remarks>
    string GetPhysicalPath(StringUdi udi);

    /// <summary>
    /// Gets the virtual path or <see cref="string.Empty"/> if not found.
    /// </summary>
    /// <param name="udi">The UDI.</param>
    /// <returns>
    /// The virtual path or <see cref="string.Empty"/> if not found.
    /// </returns>
    string GetVirtualPath(StringUdi udi);
}
