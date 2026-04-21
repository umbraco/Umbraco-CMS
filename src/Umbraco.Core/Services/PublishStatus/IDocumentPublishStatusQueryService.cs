namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Verifies the published status of documents.
/// </summary>
public interface IDocumentPublishStatusQueryService
{
    /// <summary>
    /// Checks if a document is published in a specific culture.
    /// </summary>
    /// <param name="documentKey">The document's key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>True if document is published in the specified culture.</returns>
    bool IsPublished(Guid documentKey, string culture);

    /// <summary>
    /// Checks if a document is published in any culture.
    /// </summary>
    /// <param name="documentKey">The document's key.</param>
    /// <returns>True if document has any published culture.</returns>
    bool IsPublishedInAnyCulture(Guid documentKey) => IsPublished(documentKey, string.Empty);

    /// <summary>
    /// Checks if a document is published in a specific culture.
    /// </summary>
    /// <param name="documentKey">The document's key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>True if document is published in the specified culture.</returns>
    [Obsolete("Use IsPublished instead. Scheduled for removal in Umbraco 19.")]
    bool IsDocumentPublished(Guid documentKey, string culture)
        => IsPublished(documentKey, culture);

    /// <summary>
    /// Checks if a document is published in any culture.
    /// </summary>
    /// <param name="documentKey">The document's key.</param>
    /// <returns>True if document has any published culture.</returns>
    [Obsolete("Use IsPublishedInAnyCulture instead. Scheduled for removal in Umbraco 19.")]
    bool IsDocumentPublishedInAnyCulture(Guid documentKey)
        => IsPublishedInAnyCulture(documentKey);

    /// <summary>
    /// Verifies if a document has a published ancestor path (i.e. all ancestors are themselves published in at least one culture).
    /// </summary>
    /// <param name="documentKey">The document's key.</param>
    /// <returns>True if document has a published ancestor path.</returns>
    bool HasPublishedAncestorPath(Guid documentKey);
}
