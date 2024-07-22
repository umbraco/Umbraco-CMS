namespace Umbraco.Cms.Core.Models;

public class PublishedDocumentUrl
{
    public required Guid DocumentKey { get; set; }
    public required string? Culture { get; set; }
    public required string Url { get; set; }
}
