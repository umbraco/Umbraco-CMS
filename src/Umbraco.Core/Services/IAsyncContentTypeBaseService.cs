using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IContentTypeBaseService{TItem}" />.
/// </summary>
/// <remarks>
///     This is the async-first contract used while the content-type repositories are migrated to EF Core. For now it is
///     only implemented by the document-type service; the media- and member-type services continue to use the
///     synchronous <see cref="IContentTypeBaseService{TItem}" /> until their repositories are migrated.
/// </remarks>
/// <typeparam name="TItem">The type of the item.</typeparam>
public interface IAsyncContentTypeBaseService<TItem> : IService
    where TItem : IContentTypeComposition
{
    /// <summary>
    ///     Gets a content type by its identifier.
    /// </summary>
    Task<TItem?> GetAsync(int id);

    /// <summary>
    ///     Gets a content type by its key.
    /// </summary>
    Task<TItem?> GetAsync(Guid key);

    /// <summary>
    ///     Gets a content type by its alias.
    /// </summary>
    Task<TItem?> GetAsync(string alias);

    /// <summary>
    ///     Gets the total count of content types.
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    ///     Returns a value indicating whether content nodes have been created based on the provided content type id.
    /// </summary>
    Task<bool> HasContentNodesAsync(int id);

    /// <summary>
    ///     Gets all content types.
    /// </summary>
    Task<IEnumerable<TItem>> GetAllAsync();

    /// <summary>
    ///     Gets multiple content types by their identifiers.
    /// </summary>
    Task<IEnumerable<TItem>> GetManyAsync(params int[] ids);

    /// <summary>
    ///     Gets multiple content types by their keys.
    /// </summary>
    Task<IEnumerable<TItem>> GetManyAsync(IEnumerable<Guid>? ids);

    /// <summary>
    ///     Gets all descendant content types of a given content type (parent-child axis).
    /// </summary>
    Task<IEnumerable<TItem>> GetDescendantsAsync(int id, bool andSelf);

    /// <summary>
    ///     Gets all content types that are composed of a given content type (composition axis).
    /// </summary>
    Task<IEnumerable<TItem>> GetComposedOfAsync(int id);

    /// <summary>
    ///     Gets all child content types of a given content type.
    /// </summary>
    Task<IEnumerable<TItem>> GetChildrenAsync(int id);

    /// <summary>
    ///     Gets all child content types of a given content type.
    /// </summary>
    Task<IEnumerable<TItem>> GetChildrenAsync(Guid id);

    /// <summary>
    ///     Gets a value indicating whether a content type has child content types.
    /// </summary>
    Task<bool> HasChildrenAsync(int id);

    /// <summary>
    ///     Gets a value indicating whether a content type has child content types.
    /// </summary>
    Task<bool> HasChildrenAsync(Guid id);

    /// <summary>
    ///     Gets a value indicating whether the content item with the specified path exists underneath a list view.
    /// </summary>
    Task<bool> HasContainerInPathAsync(string contentPath);

    /// <summary>
    ///     Gets a value indicating whether any of the specified content items exist underneath a list view.
    /// </summary>
    Task<bool> HasContainerInPathAsync(params int[] ids);

    /// <summary>
    ///     Creates a new content type.
    /// </summary>
    Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey);

    /// <summary>
    ///     Updates an existing content type.
    /// </summary>
    Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey);

    /// <summary>
    ///     Deletes a content type by its key.
    /// </summary>
    Task<ContentTypeOperationStatus> DeleteAsync(Guid key, Guid performingUserKey);

    /// <summary>
    ///     Deletes a content type.
    /// </summary>
    Task DeleteAsync(TItem item, Guid performingUserKey);

    /// <summary>
    ///     Deletes a collection of content types.
    /// </summary>
    Task DeleteAsync(IEnumerable<TItem> items, Guid performingUserKey);

    /// <summary>
    ///     Validates that a content type's composition is valid.
    /// </summary>
    Task<Attempt<string[]?>> ValidateCompositionAsync(TItem? compo);

    /// <summary>
    ///     Copies a content type to a container.
    /// </summary>
    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> CopyAsync(Guid key, Guid? containerKey);

    /// <summary>
    ///     Moves a content type to a container.
    /// </summary>
    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> MoveAsync(Guid key, Guid? containerKey);

    /// <summary>
    ///     Returns all the content types allowed as root.
    /// </summary>
    Task<PagedModel<TItem>> GetAllAllowedAsRootAsync(int skip, int take);

    /// <summary>
    ///     Returns all the content types allowed in the library.
    /// </summary>
    Task<PagedModel<TItem>> GetAllAllowedInLibraryAsync(int skip, int take);

    /// <summary>
    ///     Returns all content types allowed as children for a given content type key.
    /// </summary>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take);

    /// <summary>
    ///     Returns all content types allowed as children for a given content type key.
    /// </summary>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, Guid? parentContentKey, int skip, int take)
        => GetAllowedChildrenAsync(key, skip, take);

    /// <summary>
    ///     Gets the keys of all content types that allow the provided content type key as a child.
    /// </summary>
    Task<Attempt<IEnumerable<Guid>, ContentTypeOperationStatus>> GetAllowedParentKeysAsync(Guid key);
}
