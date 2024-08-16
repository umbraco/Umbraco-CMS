namespace Umbraco.Cms.Core.Models;

public class PublishedDocumentUrlSegment
{
    public required Guid DocumentKey { get; set; }
    public required int LanguageId { get; set; }
    public required string UrlSegment { get; set; }
    public required bool IsDraft { get; set; }
}
