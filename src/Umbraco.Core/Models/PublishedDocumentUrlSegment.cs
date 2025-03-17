namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a URL segment for a published document.
/// </summary>
public class PublishedDocumentUrlSegment
{
    /// <summary>
    /// Gets or sets the document key.
    /// </summary>
    public required Guid DocumentKey { get; set; }

    /// <summary>
    /// Gets or sets the language Id.
    /// </summary>
    public required int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the URL segment string.
    /// </summary>
    public required string UrlSegment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the URL segment is for a draft.
    /// </summary>
    public required bool IsDraft { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the URL segment is the primary one (first resolved from the collection of URL providers).
    /// </summary>
    public required bool IsPrimary { get; set; }
}
