using System.Globalization;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public abstract partial class ContentTypeServiceBase<TRepository, TItem>
    where TRepository : IContentTypeRepositoryBase<TItem>
    where TItem : class, IContentTypeComposition
{
    #region Containers

    /// <summary>
    /// Gets the object type GUID for content types contained by this service.
    /// </summary>
    protected abstract Guid ContainedObjectType { get; }

    /// <summary>
    /// Gets the container object type GUID.
    /// </summary>
    protected Guid ContainerObjectType => EntityContainer.GetContainerObjectType(ContainedObjectType);

    /// <summary>
    /// Creates a new entity container for organizing content types.
    /// </summary>
    /// <param name="parentId">The parent container identifier. Use -1 for root.</param>
    /// <param name="key">The unique key for the container.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="userId">The identifier of the user creating the container.</param>
    /// <returns>An attempt result containing the operation status and the created container.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds); // also for containers

        try
        {
            var container = new EntityContainer(ContainedObjectType)
            {
                Name = name,
                ParentId = parentId,
                CreatorId = userId,
                Key = key
            };

            var savingNotification = new EntityContainerSavingNotification(container, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages, container);
            }

            _containerRepository?.Save(container);
            scope.Complete();

            var savedNotification = new EntityContainerSavedNotification(container, eventMessages);
            savedNotification.WithStateFrom(savingNotification);
            scope.Notifications.Publish(savedNotification);
            // TODO: Audit trail ?

            return OperationResult.Attempt.Succeed(eventMessages, container);
        }
        catch (Exception ex)
        {
            scope.Complete();
            return OperationResult.Attempt.Fail<OperationResultType, EntityContainer>(OperationResultType.FailedCancelledByEvent, eventMessages, ex);
        }
    }

    /// <summary>
    /// Saves an entity container.
    /// </summary>
    /// <param name="container">The container to save.</param>
    /// <param name="userId">The identifier of the user saving the container.</param>
    /// <returns>An attempt result containing the operation status.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        Guid containerObjectType = ContainerObjectType;
        if (container.ContainerObjectType != containerObjectType)
        {
            var ex = new InvalidOperationException("Not a container of the proper type.");
            return OperationResult.Attempt.Fail(eventMessages, ex);
        }

        if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
        {
            var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
            return OperationResult.Attempt.Fail(eventMessages, ex);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new EntityContainerSavingNotification(container, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages);
            }

            scope.WriteLock(WriteLockIds); // also for containers

            _containerRepository?.Save(container);
            scope.Complete();

            var savedNotification = new EntityContainerSavedNotification(container, eventMessages);
            savedNotification.WithStateFrom(savingNotification);
            scope.Notifications.Publish(savedNotification);
        }

        // TODO: Audit trail ?

        return OperationResult.Attempt.Succeed(eventMessages);
    }

    /// <summary>
    /// Gets an entity container by its integer identifier.
    /// </summary>
    /// <param name="containerId">The integer identifier of the container.</param>
    /// <returns>The entity container if found; otherwise, null.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public EntityContainer? GetContainer(int containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    /// <summary>
    /// Gets an entity container by its GUID identifier.
    /// </summary>
    /// <param name="containerId">The GUID identifier of the container.</param>
    /// <returns>The entity container if found; otherwise, null.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public EntityContainer? GetContainer(Guid containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    /// <summary>
    /// Gets entity containers by their integer identifiers.
    /// </summary>
    /// <param name="containerIds">The array of container identifiers.</param>
    /// <returns>A collection of entity containers.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.GetMany(containerIds);
    }

    /// <summary>
    /// Gets the ancestor containers of the specified content type item.
    /// </summary>
    /// <param name="item">The content type item to get ancestor containers for.</param>
    /// <returns>A collection of ancestor entity containers.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<EntityContainer> GetContainers(TItem item)
    {
        var ancestorIds = item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt) ? asInt : int.MinValue)
            .Where(x => x != int.MinValue && x != item.Id)
            .ToArray();

        return GetContainers(ancestorIds);
    }

    /// <summary>
    /// Gets entity containers by name and level.
    /// </summary>
    /// <param name="name">The name of the containers to find.</param>
    /// <param name="level">The level of the containers in the hierarchy.</param>
    /// <returns>A collection of entity containers matching the criteria.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<EntityContainer> GetContainers(string name, int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(name, level);
    }

    /// <summary>
    /// Deletes an entity container.
    /// </summary>
    /// <param name="containerId">The identifier of the container to delete.</param>
    /// <param name="userId">The identifier of the user deleting the container.</param>
    /// <returns>An attempt result containing the operation status.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds); // also for containers

        EntityContainer? container = _containerRepository?.Get(containerId);
        if (container == null)
        {
            return OperationResult.Attempt.NoOperation(eventMessages);
        }

        // 'container' here does not know about its children, so we need
        // to get it again from the entity repository, as a light entity
        IEntitySlim? entity = _entityRepository.Get(container.Id);
        if (entity?.HasChildren ?? false)
        {
            scope.Complete();
            return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, eventMessages));
        }

        var deletingNotification = new EntityContainerDeletingNotification(container, eventMessages);
        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, eventMessages));
        }

        _containerRepository?.Delete(container);
        scope.Complete();

        var deletedNotification = new EntityContainerDeletedNotification(container, eventMessages);
        deletedNotification.WithStateFrom(deletingNotification);
        scope.Notifications.Publish(deletedNotification);

        return OperationResult.Attempt.Succeed(eventMessages);
        // TODO: Audit trail ?
    }

    /// <summary>
    /// Renames an entity container.
    /// </summary>
    /// <param name="id">The identifier of the container to rename.</param>
    /// <param name="name">The new name for the container.</param>
    /// <param name="userId">The identifier of the user renaming the container.</param>
    /// <returns>An attempt result containing the operation status and the renamed container.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Scheduled for removal in Umbraco 18.")]
    public Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(WriteLockIds); // also for containers

            try
            {
                EntityContainer? container = _containerRepository?.Get(id);

                //throw if null, this will be caught by the catch and a failed returned
                if (container == null)
                {
                    throw new InvalidOperationException("No container found with id " + id);
                }

                container.Name = name;

                var renamingNotification = new EntityContainerRenamingNotification(container, eventMessages);
                if (scope.Notifications.PublishCancelable(renamingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel<EntityContainer>(eventMessages);
                }

                _containerRepository?.Save(container);
                scope.Complete();

                var renamedNotification = new EntityContainerRenamedNotification(container, eventMessages);
                renamedNotification.WithStateFrom(renamingNotification);
                scope.Notifications.Publish(renamedNotification);

                return OperationResult.Attempt.Succeed(OperationResultType.Success, eventMessages, container);
            }
            catch (Exception ex)
            {
                return OperationResult.Attempt.Fail<EntityContainer>(eventMessages, ex);
            }
        }
    }

    #endregion
}
