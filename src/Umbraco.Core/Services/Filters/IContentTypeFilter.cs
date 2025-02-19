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
        where TItem : IContentTypeComposition
        => Task.FromResult(contentTypes);

    /// <summary>
    /// Filters the content types retrieved for being allowed as children of a parent content type.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentKey">The parent content key.</param>
    /// <returns>Filtered collection of content types.</returns>
    [Obsolete("Please the method overload taking all parameters. This method will be removed in Umbraco 17.")]
    Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentKey)
        => Task.FromResult(contentTypes);

    /// <summary>
    /// Filters the content types retrieved for being allowed as children of a parent content type.
    /// </summary>
    /// <param name="contentTypes">Retrieved collection of content types.</param>
    /// <param name="parentContentTypeKey">The parent content type key.</param>
    /// <param name="parentContentKey">The parent content key (provided to allow for custom filtering of the returned list of children based on the content context).</param>
    /// <returns>Filtered collection of content types.</returns>
    Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentContentTypeKey, Guid? parentContentKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => FilterAllowedChildrenAsync(contentTypes, parentContentTypeKey);
#pragma warning restore CS0618 // Type or member is obsolete
}
