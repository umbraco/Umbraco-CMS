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
    /// <remarks>
    /// This property returns 0 for invariant content. Use <see cref="NullableLanguageId"/> instead,
    /// which correctly returns <c>null</c> for invariant content.
    /// </remarks>
    [Obsolete("Use NullableLanguageId instead. This property returns 0 for invariant content. Scheduled for removal in Umbraco 18, when the NullableLanguageId will also be renamed to LanguageId.")]
    public int LanguageId
    {
        get => NullableLanguageId ?? 0;
        set => NullableLanguageId = value;
    }

    /// <summary>
    /// Gets or sets the language Id. NULL indicates invariant content (not language-specific).
    /// </summary>
    public required int? NullableLanguageId { get; set; }

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
