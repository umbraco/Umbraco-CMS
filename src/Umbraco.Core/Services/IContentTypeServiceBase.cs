using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides a common base interface for <see cref="IContentTypeBase" />.
/// </summary>
public interface IContentTypeBaseService
{
    /// <summary>
    ///     Gets a content type.
    /// </summary>
    IContentTypeComposition? Get(int id);
}

/// <summary>
///     Provides a common base interface for <see cref="IContentTypeService" />, <see cref="IMediaTypeService" /> and
///     <see cref="IMemberTypeService" />.
/// </summary>
/// <typeparam name="TItem">The type of the item.</typeparam>
public interface IContentTypeBaseService<TItem> : IContentTypeBaseService, IService
    where TItem : IContentTypeComposition
{
    /// <summary>
    ///     Gets a content type.
    /// </summary>
    new TItem? Get(int id);

    /// <summary>
    ///     Gets a content type.
    /// </summary>
    TItem? Get(Guid key);

    /// <summary>
    /// Gets a content type.
    /// </summary>
    /// <param name="guid">The key of the content type.</param>
    /// <returns>The found content type, null if none was found.</returns>
    Task<TItem?> GetAsync(Guid guid);

    /// <summary>
    ///     Gets a content type.
    /// </summary>
    TItem? Get(string alias);

    int Count();

    /// <summary>
    ///     Returns true or false depending on whether content nodes have been created based on the provided content type id.
    /// </summary>
    bool HasContentNodes(int id);

    IEnumerable<TItem> GetAll();
    IEnumerable<TItem> GetMany(params int[] ids);

    IEnumerable<TItem> GetMany(IEnumerable<Guid>? ids);

    IEnumerable<TItem> GetDescendants(int id, bool andSelf); // parent-child axis

    IEnumerable<TItem> GetComposedOf(int id); // composition axis

    IEnumerable<TItem> GetChildren(int id);

    IEnumerable<TItem> GetChildren(Guid id);

    bool HasChildren(int id);

    bool HasChildren(Guid id);

    [Obsolete("Please use the respective Create or Update instead")]
    void Save(TItem? item, int userId = Constants.Security.SuperUserId);

    [Obsolete("Please use the respective Create or Update instead")]
    Task SaveAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.CompletedTask;
    }

    [Obsolete("Please use the respective Create or Update instead")]
    void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId);

    Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.FromResult(Attempt.Succeed(ContentTypeOperationStatus.Success));
    }

    Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.FromResult(Attempt.Succeed(ContentTypeOperationStatus.Success));
    }

    void Delete(TItem item, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="key">The item to delete.</param>
    /// <param name="performingUserKey"></param>
    /// <returns></returns>
    Task<ContentTypeOperationStatus> DeleteAsync(Guid key, Guid performingUserKey);

    void Delete(IEnumerable<TItem> item, int userId = Constants.Security.SuperUserId);

    Attempt<string[]?> ValidateComposition(TItem? compo);

    /// <summary>
    ///     Given the path of a content item, this will return true if the content item exists underneath a list view content
    ///     item
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    bool HasContainerInPath(string contentPath);

    /// <summary>
    ///     Gets a value indicating whether there is a list view content item in the path.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    bool HasContainerInPath(params int[] ids);

    Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentContainerId, Guid key, string name, int userId = Constants.Security.SuperUserId);

    Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

    EntityContainer? GetContainer(int containerId);

    EntityContainer? GetContainer(Guid containerId);

    IEnumerable<EntityContainer> GetContainers(int[] containerIds);

    IEnumerable<EntityContainer> GetContainers(TItem contentType);

    IEnumerable<EntityContainer> GetContainers(string folderName, int level);

    Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

    Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

    [Obsolete("Please use MoveAsync. Will be removed in V16.")]
    Attempt<OperationResult<MoveOperationStatusType>?> Move(TItem moving, int containerId);

    [Obsolete("Please use CopyAsync. Will be removed in V16.")]
    Attempt<OperationResult<MoveOperationStatusType, TItem>?> Copy(TItem copying, int containerId);

    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    TItem Copy(TItem original, string alias, string name, int parentId = -1);

    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    TItem Copy(TItem original, string alias, string name, TItem parent);

    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> CopyAsync(Guid key, Guid? containerKey);

    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> MoveAsync(Guid key, Guid? containerKey);

    /// <summary>
    /// Applies inheritance to a content type.
    /// </summary>
    /// <param name="key">The content type key.</param>
    /// <param name="parentKey">The intended parent to inherit from's key, or null if inheritance is being cleared.</param>
    /// <param name="performingUserKey">The key of the performing user.</param>
    /// <returns>Status of operation result.</returns>
    Task<Attempt<ContentTypeOperationStatus>> InheritAsync(Guid key, Guid? parentKey, Guid performingUserKey)
        => throw new NotSupportedException();

    /// <summary>
    /// Returns all the content type allowed as root.
    /// </summary>
    Task<PagedModel<TItem>> GetAllAllowedAsRootAsync(int skip, int take);

    /// <summary>
    /// Returns all content types allowed as children for a given content type key.
    /// </summary>
    /// <param name="key">The content type key.</param>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take);

    /// <summary>
    /// Returns all content types allowed as children for a given content type key.
    /// </summary>
    /// <param name="key">The content type key.</param>
    /// <param name="parentContentKey">The parent content key.</param>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, Guid? parentContentKey, int skip, int take)
        => GetAllowedChildrenAsync(key, skip, take);

}
