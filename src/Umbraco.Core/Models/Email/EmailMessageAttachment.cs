namespace Umbraco.Cms.Core.Models.Email;

public class EmailMessageAttachment
{
    public EmailMessageAttachment(Stream stream, string fileName)
    {
        Stream = stream;
        FileName = fileName;
    }

    public Stream Stream { get; }

    public string FileName { get; }
}
