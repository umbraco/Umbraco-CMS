namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Verifies the published status of elements.
/// </summary>
public interface IElementPublishStatusQueryService
{
    /// <summary>
    /// Checks if an element is published in a specific culture.
    /// </summary>
    /// <param name="elementKey">The element's key.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>True if element is published in the specified culture.</returns>
    bool IsPublished(Guid elementKey, string culture);

    /// <summary>
    /// Checks if an element is published in any culture.
    /// </summary>
    /// <param name="elementKey">The element's key.</param>
    /// <returns>True if element has any published culture.</returns>
    bool IsPublishedInAnyCulture(Guid elementKey) => IsPublished(elementKey, string.Empty);
}
