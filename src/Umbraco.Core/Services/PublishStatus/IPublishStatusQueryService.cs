namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///
/// </summary>
public interface IPublishStatusQueryService
{
    bool IsDocumentPublished(Guid documentKey, string culture);

    /// <summary>
    /// Checks if a document is published in any culture.
    /// </summary>
    /// <param name="documentKey">Key to check for.</param>
    /// <returns>True if document has any published culture.</returns>
    bool IsDocumentPublishedInAnyCulture(Guid documentKey) => IsDocumentPublished(documentKey, string.Empty);
}
