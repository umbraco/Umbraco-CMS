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
    /// <typeparam name="TItem">The type of content type, which must implement <see cref="IContentTypeComposition"/>.</typeparam>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
        where TItem : IContentTypeComposition
        => Task.FromResult(contentTypes);

    /// <summary>
    /// Filters the content types retrieved for being allowed in the library.
    /// </summary>
    /// <typeparam name="TItem">The type of content type, which must implement <see cref="IContentTypeComposition"/>.</typeparam>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <returns>Filtered collection of content types.</returns>
    [Obsolete("Please use the method overload taking all parameters. Scheduled for removal in Umbraco 20.")]
    Task<IEnumerable<TItem>> FilterAllowedInLibraryAsync<TItem>(IEnumerable<TItem> contentTypes)
        where TItem : IContentTypeComposition
        => Task.FromResult(contentTypes);

    /// <summary>
    /// Filters the content types retrieved for being allowed in the library.
    /// </summary>
    /// <typeparam name="TItem">The type of content type, which must implement <see cref="IContentTypeComposition"/>.</typeparam>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentKey">The parent container key (provided to allow for custom filtering of the returned list based on the library context), or <c>null</c> at the library root.</param>
    /// <returns>Filtered collection of content types.</returns>
    // TODO (V20): Remove the default implementation when the obsolete FilterAllowedInLibraryAsync overload is removed.
    Task<IEnumerable<TItem>> FilterAllowedInLibraryAsync<TItem>(IEnumerable<TItem> contentTypes, Guid? parentKey)
        where TItem : IContentTypeComposition
#pragma warning disable CS0618 // Type or member is obsolete
        => FilterAllowedInLibraryAsync(contentTypes);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Filters the content types retrieved for being allowed as children of a parent content type.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentContentTypeKey">The parent content type key.</param>
    /// <param name="parentContentKey">The parent content key (provided to allow for custom filtering of the returned list of children based on the content context).</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentContentTypeKey, Guid? parentContentKey)
        => Task.FromResult(contentTypes);
}
