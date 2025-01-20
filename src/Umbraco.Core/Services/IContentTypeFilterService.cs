using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines a service for filtering content types after retrieval.
/// </summary>
public interface IContentTypeFilterService
{
    /// <summary>
    /// Filters the content types retrieved for being allowed at the root.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<IContentTypeComposition>> FilterAllowedAtRootAsync(IEnumerable<IContentTypeComposition> contentTypes);

    /// <summary>
    /// Filters the content types retrieved for being allowed as children of a parent content type.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentKey">The parent content type key.</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentKey);
}
