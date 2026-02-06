namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email attachment with its content stream and file name.
/// </summary>
public class EmailMessageAttachment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailMessageAttachment" /> class.
    /// </summary>
    /// <param name="stream">The stream containing the attachment content.</param>
    /// <param name="fileName">The name of the attachment file.</param>
    public EmailMessageAttachment(Stream stream, string fileName)
    {
        Stream = stream;
        FileName = fileName;
    }

    /// <summary>
    ///     Gets the stream containing the attachment content.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    ///     Gets the name of the attachment file.
    /// </summary>
    public string FileName { get; }
}
