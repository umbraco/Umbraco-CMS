namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a URL alias for a published document.
/// </summary>
public class PublishedDocumentUrlAlias
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
    /// Gets or sets the normalized URL alias (lowercase, no leading slash).
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the root ancestor key for domain scoping.
    /// </summary>
    public required Guid RootAncestorKey { get; set; }
}
