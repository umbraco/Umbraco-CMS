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

    /// <summary>
    ///     Gets the total count of content types.
    /// </summary>
    /// <returns>The count of content types.</returns>
    int Count();

    /// <summary>
    ///     Returns true or false depending on whether content nodes have been created based on the provided content type id.
    /// </summary>
    /// <param name="id">The identifier of the content type.</param>
    /// <returns><c>true</c> if content nodes exist for this content type; otherwise, <c>false</c>.</returns>
    bool HasContentNodes(int id);

    /// <summary>
    ///     Gets all content types.
    /// </summary>
    /// <returns>A collection of all content types.</returns>
    IEnumerable<TItem> GetAll();

    /// <summary>
    ///     Gets multiple content types by their identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the content types to retrieve.</param>
    /// <returns>A collection of content types.</returns>
    IEnumerable<TItem> GetMany(params int[] ids);

    /// <summary>
    ///     Gets multiple content types by their unique identifiers.
    /// </summary>
    /// <param name="ids">The unique identifiers of the content types to retrieve.</param>
    /// <returns>A collection of content types.</returns>
    IEnumerable<TItem> GetMany(IEnumerable<Guid>? ids);

    /// <summary>
    ///     Gets all descendant content types of a given content type.
    /// </summary>
    /// <param name="id">The identifier of the parent content type.</param>
    /// <param name="andSelf">Whether to include the parent content type itself.</param>
    /// <returns>A collection of descendant content types.</returns>
    IEnumerable<TItem> GetDescendants(int id, bool andSelf); // parent-child axis

    /// <summary>
    ///     Gets all content types that are composed of a given content type.
    /// </summary>
    /// <param name="id">The identifier of the composition content type.</param>
    /// <returns>A collection of content types that use the specified composition.</returns>
    IEnumerable<TItem> GetComposedOf(int id); // composition axis

    /// <summary>
    ///     Gets all child content types of a given content type.
    /// </summary>
    /// <param name="id">The identifier of the parent content type.</param>
    /// <returns>A collection of child content types.</returns>
    IEnumerable<TItem> GetChildren(int id);

    /// <summary>
    ///     Gets all child content types of a given content type.
    /// </summary>
    /// <param name="id">The unique identifier of the parent content type.</param>
    /// <returns>A collection of child content types.</returns>
    IEnumerable<TItem> GetChildren(Guid id);

    /// <summary>
    ///     Checks if a content type has child content types.
    /// </summary>
    /// <param name="id">The identifier of the content type.</param>
    /// <returns><c>true</c> if the content type has children; otherwise, <c>false</c>.</returns>
    bool HasChildren(int id);

    /// <summary>
    ///     Checks if a content type has child content types.
    /// </summary>
    /// <param name="id">The unique identifier of the content type.</param>
    /// <returns><c>true</c> if the content type has children; otherwise, <c>false</c>.</returns>
    bool HasChildren(Guid id);

    /// <summary>
    ///     Saves a content type.
    /// </summary>
    /// <param name="item">The content type to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    [Obsolete("Please use the respective Create or Update instead")]
    void Save(TItem? item, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a content type asynchronously.
    /// </summary>
    /// <param name="item">The content type to save.</param>
    /// <param name="performingUserKey">The unique identifier of the user performing the action.</param>
    [Obsolete("Please use the respective Create or Update instead")]
    Task SaveAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Saves a collection of content types.
    /// </summary>
    /// <param name="items">The content types to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    [Obsolete("Please use the respective Create or Update instead")]
    void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a new content type.
    /// </summary>
    /// <param name="item">The content type to create.</param>
    /// <param name="performingUserKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation status.</returns>
    Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.FromResult(Attempt.Succeed(ContentTypeOperationStatus.Success));
    }

    /// <summary>
    ///     Updates an existing content type.
    /// </summary>
    /// <param name="item">The content type to update.</param>
    /// <param name="performingUserKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation status.</returns>
    Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey)
    {
        Save(item);
        return Task.FromResult(Attempt.Succeed(ContentTypeOperationStatus.Success));
    }

    /// <summary>
    ///     Deletes a content type.
    /// </summary>
    /// <param name="item">The content type to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void Delete(TItem item, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a content type by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content type to delete.</param>
    /// <param name="performingUserKey">The unique identifier of the user performing the action.</param>
    /// <returns>The operation status.</returns>
    Task<ContentTypeOperationStatus> DeleteAsync(Guid key, Guid performingUserKey);

    /// <summary>
    ///     Deletes a collection of content types.
    /// </summary>
    /// <param name="item">The content types to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    void Delete(IEnumerable<TItem> item, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Validates that a content type's composition is valid.
    /// </summary>
    /// <param name="compo">The content type composition to validate.</param>
    /// <returns>An attempt containing an array of error messages if validation fails, or <c>null</c> if valid.</returns>
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

    /// <summary>
    ///     Creates a new container (folder) for organizing content types.
    /// </summary>
    /// <param name="parentContainerId">The identifier of the parent container, or -1 for root level.</param>
    /// <param name="key">The unique identifier for the new container.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation result with the created container.</returns>
    Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentContainerId, Guid key, string name, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a container.
    /// </summary>
    /// <param name="container">The container to save.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation result.</returns>
    Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a container by its identifier.
    /// </summary>
    /// <param name="containerId">The identifier of the container.</param>
    /// <returns>The container, or <c>null</c> if not found.</returns>
    EntityContainer? GetContainer(int containerId);

    /// <summary>
    ///     Gets a container by its unique identifier.
    /// </summary>
    /// <param name="containerId">The unique identifier of the container.</param>
    /// <returns>The container, or <c>null</c> if not found.</returns>
    EntityContainer? GetContainer(Guid containerId);

    /// <summary>
    ///     Gets multiple containers by their identifiers.
    /// </summary>
    /// <param name="containerIds">The identifiers of the containers to retrieve.</param>
    /// <returns>A collection of containers.</returns>
    IEnumerable<EntityContainer> GetContainers(int[] containerIds);

    /// <summary>
    ///     Gets all containers in the path of a content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>A collection of containers in the content type's path.</returns>
    IEnumerable<EntityContainer> GetContainers(TItem contentType);

    /// <summary>
    ///     Gets containers by name and level.
    /// </summary>
    /// <param name="folderName">The name of the folder.</param>
    /// <param name="level">The level in the hierarchy.</param>
    /// <returns>A collection of matching containers.</returns>
    IEnumerable<EntityContainer> GetContainers(string folderName, int level);

    /// <summary>
    ///     Deletes a container.
    /// </summary>
    /// <param name="containerId">The identifier of the container to delete.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation result.</returns>
    Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Renames a container.
    /// </summary>
    /// <param name="id">The identifier of the container to rename.</param>
    /// <param name="name">The new name for the container.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation result with the renamed container.</returns>
    Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Moves a content type to a container.
    /// </summary>
    /// <param name="moving">The content type to move.</param>
    /// <param name="containerId">The identifier of the target container.</param>
    /// <returns>An attempt containing the operation result.</returns>
    [Obsolete("Please use MoveAsync. Will be removed in V16.")]
    Attempt<OperationResult<MoveOperationStatusType>?> Move(TItem moving, int containerId);

    /// <summary>
    ///     Copies a content type to a container.
    /// </summary>
    /// <param name="copying">The content type to copy.</param>
    /// <param name="containerId">The identifier of the target container.</param>
    /// <returns>An attempt containing the operation result with the copied content type.</returns>
    [Obsolete("Please use CopyAsync. Will be removed in V16.")]
    Attempt<OperationResult<MoveOperationStatusType, TItem>?> Copy(TItem copying, int containerId);

    /// <summary>
    ///     Copies a content type with a new alias and name.
    /// </summary>
    /// <param name="original">The content type to copy.</param>
    /// <param name="alias">The alias for the copy.</param>
    /// <param name="name">The name for the copy.</param>
    /// <param name="parentId">The identifier of the parent container.</param>
    /// <returns>The copied content type.</returns>
    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    TItem Copy(TItem original, string alias, string name, int parentId = -1);

    /// <summary>
    ///     Copies a content type with a new alias and name under a parent.
    /// </summary>
    /// <param name="original">The content type to copy.</param>
    /// <param name="alias">The alias for the copy.</param>
    /// <param name="name">The name for the copy.</param>
    /// <param name="parent">The parent content type.</param>
    /// <returns>The copied content type.</returns>
    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    TItem Copy(TItem original, string alias, string name, TItem parent);

    /// <summary>
    ///     Copies a content type to a container asynchronously.
    /// </summary>
    /// <param name="key">The unique identifier of the content type to copy.</param>
    /// <param name="containerKey">The unique identifier of the target container, or <c>null</c> for the root.</param>
    /// <returns>An attempt containing the copied content type or an error status.</returns>
    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> CopyAsync(Guid key, Guid? containerKey);

    /// <summary>
    ///     Moves a content type to a container asynchronously.
    /// </summary>
    /// <param name="key">The unique identifier of the content type to move.</param>
    /// <param name="containerKey">The unique identifier of the target container, or <c>null</c> for the root.</param>
    /// <returns>An attempt containing the moved content type or an error status.</returns>
    Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> MoveAsync(Guid key, Guid? containerKey);

    /// <summary>
    /// Returns all the content type allowed as root.
    /// </summary>
    Task<PagedModel<TItem>> GetAllAllowedAsRootAsync(int skip, int take);

    /// <summary>
    /// Returns all content types allowed as children for a given content type key.
    /// </summary>
    /// <param name="key">The content type key.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take);

    /// <summary>
    /// Returns all content types allowed as children for a given content type key.
    /// </summary>
    /// <param name="key">The content type key.</param>
    /// <param name="parentContentKey">The parent content key.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, Guid? parentContentKey, int skip, int take)
        => GetAllowedChildrenAsync(key, skip, take);

}
