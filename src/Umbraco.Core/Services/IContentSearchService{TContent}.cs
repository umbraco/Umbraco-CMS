using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides search services for content items of a specific type.
/// </summary>
/// <typeparam name="TContent">The type of content to search for.</typeparam>
public interface IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Searches for child content items under a specified parent.
    /// </summary>
    /// <param name="query">The search query string to filter results.</param>
    /// <param name="parentId">The unique identifier of the parent content item, or <c>null</c> to search at the root level.</param>
    /// <param name="ordering">The ordering configuration for the results.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to take for pagination.</param>
    /// <returns>A paged model containing the matching content items.</returns>
    Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        Ordering? ordering,
        int skip = 0,
        int take = 100);
}
