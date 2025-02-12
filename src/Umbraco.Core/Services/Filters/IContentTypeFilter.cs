using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Filters;

/// <summary>
/// Defines methods for filtering content types after retrieval from the database.
/// </summary>
public interface IContentTypeFilter
{
    /// <summary>
    /// Filters the content types retrieved for being allowed at the root.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
        where TItem : IContentTypeComposition;

    /// <summary>
    /// Filters the content types retrieved for being allowed as children of a parent content type.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentKey">The parent content type key.</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentKey);
}
