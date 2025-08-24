namespace Umbraco.Cms.Core.Models.Email
{
    public class EmailMessageLinkedResource(Stream stream, string fileName, string contentId)
    {
        public Stream Stream { get; } = stream;

        public string FileName { get; } = fileName;

        public string ContentId { get; } = contentId;
    }
}
