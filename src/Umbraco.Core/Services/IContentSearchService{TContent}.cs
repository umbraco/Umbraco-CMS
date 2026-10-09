using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides search services for content items of a specific type.
/// </summary>
/// <typeparam name="TContent">The type of content item to search for. Must implement <see cref="IContentBase"/>.</typeparam>
public interface IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    /// <summary>
    ///     Searches for children of a content item with optional property filtering.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="parentId">The parent content item key.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load. If null, all properties are loaded.
    ///     If empty array, no custom properties are loaded.
    /// </param>
    /// <param name="ordering">The ordering.</param>
    /// <param name="loadTemplates">
    ///     Whether to load templates. Set to false for performance optimization when templates are not needed
    ///     (e.g., collection views). Default is true. Only applies to Document content; ignored for Media/Member.
    /// </param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A paged model of content items.</returns>
    Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        string[]? propertyAliases,
        Ordering? ordering,
        bool loadTemplates = true,
        int skip = 0,
        int take = 100);
}
