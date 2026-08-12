using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides base functionality for entity type container services that manage folder hierarchies for content types.
/// </summary>
/// <typeparam name="TTreeEntity">The type of tree entity contained within the containers.</typeparam>
/// <typeparam name="TEntityContainerRepository">The type of repository used for container operations.</typeparam>
/// <remarks>
///     Containers (folders) are used to organize content types, media types, data types, and other entities
///     in a hierarchical structure within the Umbraco backoffice.
/// </remarks>
internal abstract class EntityTypeContainerService<TTreeEntity, TEntityContainerRepository> : RepositoryService, IEntityTypeContainerService<TTreeEntity>
    where TTreeEntity : ITreeEntity
    where TEntityContainerRepository : IEntityContainerRepository
{
    private readonly TEntityContainerRepository _entityContainerRepository;
    private readonly IAuditService _auditService;
    private readonly IEntityRepository _entityRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     The number of descendants fetched per iteration when moving a container.
    /// </summary>
    /// <remarks>Internal so the tests can reach it.</remarks>
    internal const int DescendantsIteratorPageSize = 500;

    /// <summary>
    ///     Gets the entity service, used to enumerate the descendants of a container.
    /// </summary>
    protected IEntityService EntityService { get; }

    /// <summary>
    ///     Gets the GUID identifying the type of objects contained within these containers.
    /// </summary>
    protected abstract Guid ContainedObjectType { get; }

    /// <summary>
    ///     Gets the Umbraco object type for the container itself.
    /// </summary>
    protected abstract UmbracoObjectTypes ContainerObjectType { get; }

    /// <summary>
    ///     Gets the lock identifiers required for read operations.
    /// </summary>
    protected abstract int[] ReadLockIds { get; }

    /// <summary>
    ///     Gets the lock identifiers required for write operations.
    /// </summary>
    protected abstract int[] WriteLockIds { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityTypeContainerService{TTreeEntity, TEntityContainerRepository}" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="entityContainerRepository">The repository for container data access.</param>
    /// <param name="auditService">The audit service for logging operations.</param>
    /// <param name="entityRepository">The entity repository for general entity operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    /// <param name="entityService">The entity service, used to enumerate the descendants of a container.</param>
    protected EntityTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TEntityContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityService entityService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _entityContainerRepository = entityContainerRepository;
        _auditService = auditService;
        _entityRepository = entityRepository;
        _userIdKeyResolver = userIdKeyResolver;
        EntityService = entityService;
    }

    /// <summary>
    ///     Gets a contained (leaf) entity by its node ID, so its structural data can be rewritten as part of a
    ///     container move.
    /// </summary>
    /// <param name="id">The node ID of the entity.</param>
    /// <returns>The entity, or null if it does not exist.</returns>
    protected abstract TTreeEntity? GetContainedEntity(int id);

    /// <summary>
    ///     Persists a contained (leaf) entity whose structural data was rewritten by a container move.
    /// </summary>
    /// <param name="entity">The entity to persist.</param>
    protected abstract void SaveContainedEntity(TTreeEntity entity);

    /// <summary>
    ///     Applies any tree specific state to a contained (leaf) entity before it is persisted as part of a container
    ///     move - for example recycle bin state.
    /// </summary>
    /// <param name="entity">The entity being moved.</param>
    /// <param name="trash">Whether the entity is being moved to the recycle bin.</param>
    /// <param name="userKey">Key of the user issuing the move.</param>
    /// <returns>
    ///     <see cref="EntityContainerOperationStatus.Success" /> to continue, or any other status to abort the move.
    /// </returns>
    protected virtual Task<EntityContainerOperationStatus> PrepareContainedEntityForMoveAsync(TTreeEntity entity, bool trash, Guid userKey)
        => Task.FromResult(EntityContainerOperationStatus.Success);

    /// <summary>
    ///     Publishes the tree specific notifications for the contained (leaf) entities moved by a container move.
    ///     Called within the scope, after the move has been persisted.
    /// </summary>
    /// <param name="scope">The scope the move is running in.</param>
    /// <param name="movedEntities">
    ///     The moved entities, each carrying the path it had before the move. No new parent key is supplied, as a
    ///     container move re-homes the container, leaving the parent of every entity within it unchanged.
    /// </param>
    /// <param name="eventMessages">The event messages for the operation.</param>
    protected virtual void PublishContainedEntitiesMovedNotifications(ICoreScope scope, IReadOnlyCollection<MoveEventInfo<TTreeEntity>> movedEntities, EventMessages eventMessages)
    {
    }

    /// <inheritdoc />
    public Task<EntityContainer?> GetAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return Task.FromResult(_entityContainerRepository.Get(id));
    }


    /// <inheritdoc />
    public Task<IEnumerable<EntityContainer>> GetAsync(string name, int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return Task.FromResult(_entityContainerRepository.Get(name, level));
    }
    /// <inheritdoc />
    public Task<IEnumerable<EntityContainer>> GetAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return Task.FromResult(_entityContainerRepository.GetMany());
    }

    /// <inheritdoc />
    public Task<EntityContainer?> GetParentAsync(EntityContainer container)
        => Task.FromResult(GetParent(container));

    /// <inheritdoc />
    public Task<EntityContainer?> GetParentAsync(TTreeEntity entity)
        => Task.FromResult(GetParent(entity));

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> CreateAsync(Guid? key, string name, Guid? parentKey, Guid userKey)
    {
        var container = new EntityContainer(ContainedObjectType) { Name = name };
        if (key.HasValue)
        {
            container.Key = key.Value;
        }

        return await SaveAsync(
            container,
            userKey,
            () =>
            {
                if (container.Id > 0)
                {
                    return EntityContainerOperationStatus.InvalidId;
                }

                if (_entityContainerRepository.Get(container.Key) is not null)
                {
                    return EntityContainerOperationStatus.DuplicateKey;
                }

                EntityContainer? parentContainer = parentKey.HasValue
                    ? _entityContainerRepository.Get(parentKey.Value)
                    : null;

                if (parentKey.HasValue && parentContainer == null)
                {
                    return EntityContainerOperationStatus.ParentNotFound;
                }

                if (_entityContainerRepository.HasDuplicateName(parentContainer?.Id ?? Constants.System.Root, container.Name!))
                {
                    return EntityContainerOperationStatus.DuplicateName;
                }

                container.ParentId = parentContainer?.Id ?? Constants.System.Root;
                return EntityContainerOperationStatus.Success;
            },
            AuditType.New);
    }

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> UpdateAsync(Guid key, string name, Guid userKey)
    {
        EntityContainer? container = await GetAsync(key);
        if (container is null)
        {
            return Attempt.FailWithStatus(EntityContainerOperationStatus.NotFound, container);
        }

        container.Name = name;

        return await SaveAsync(
            container,
            userKey,
            () =>
            {
                if (container.Id == 0)
                {
                    return EntityContainerOperationStatus.InvalidId;
                }

                if (container.IsPropertyDirty(nameof(EntityContainer.ParentId)))
                {
                    LoggerFactory.CreateLogger(GetType()).LogWarning(
                        $"Cannot use {nameof(UpdateAsync)} to change the container parent. Move the container instead.");
                    return EntityContainerOperationStatus.ParentChangeNotAllowed;
                }

                return EntityContainerOperationStatus.Success;
            },
            AuditType.Save);
    }

    /// <inheritdoc />
    public virtual async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        WriteLock(scope);

        EntityContainer? container = _entityContainerRepository.Get(id);
        if (container == null)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotFound, null);
        }

        // 'container' here does not know about its children, so we need
        // to get it again from the entity repository, as a light entity
        IEntitySlim? entity = _entityRepository.Get(container.Id);
        if (entity?.HasChildren is true)
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotEmpty, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingEntityContainerNotification = new EntityContainerDeletingNotification(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingEntityContainerNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.CancelledByNotification, container);
        }

        _entityContainerRepository.Delete(container);

        await AuditAsync(AuditType.Delete, userKey, container.Id);
        scope.Complete();

        scope.Notifications.Publish(new EntityContainerDeletedNotification(container, eventMessages).WithStateFrom(deletingEntityContainerNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
    }

    /// <inheritdoc />
    public virtual async Task<Attempt<EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    /// <summary>
    ///     Moves a container to a new parent container, optionally requiring the container to be in the recycle bin
    ///     (i.e. a restore).
    /// </summary>
    /// <param name="key">The key of the container to move.</param>
    /// <param name="parentKey">The key of the parent container to move to, or null to move to the tree root.</param>
    /// <param name="userKey">Key of the user issuing the move.</param>
    /// <param name="mustBeInRecycleBin">Whether the container is required to be in the recycle bin.</param>
    /// <returns>An <see cref="Attempt{TStatus}" /> describing the outcome of the operation.</returns>
    protected async Task<Attempt<EntityContainerOperationStatus>> HandleMoveAsync(
        Guid key,
        Guid? parentKey,
        Guid userKey,
        bool mustBeInRecycleBin = false)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        WriteLock(scope);

        EntityContainer? container = _entityContainerRepository.Get(key);
        if (container is null)
        {
            return Attempt.Fail(EntityContainerOperationStatus.NotFound);
        }

        if (mustBeInRecycleBin && container.Trashed is false)
        {
            return Attempt.Fail(EntityContainerOperationStatus.NotInTrash);
        }

        var parentId = Constants.System.Root;
        var parentPath = parentId.ToString();
        var parentLevel = 0;
        if (parentKey.HasValue && parentKey.Value != Guid.Empty)
        {
            EntityContainer? parent = _entityContainerRepository.Get(parentKey.Value);
            if (parent is null)
            {
                return Attempt.Fail(EntityContainerOperationStatus.ParentNotFound);
            }

            if (parent.Trashed)
            {
                // Cannot move to a trashed container.
                return Attempt.Fail(EntityContainerOperationStatus.InTrash);
            }

            parentId = parent.Id;
            parentPath = parent.Path;
            parentLevel = parent.Level;
        }

        var originalPath = container.Path;
        Attempt<EntityContainerOperationStatus> moveResult = await MoveLockedAsync(
            scope,
            key,
            parentId,
            parentPath,
            parentLevel,
            false,
            userKey,
            cont => parentPath.IsDescendantOrSelfOfPath(cont.Path)
                ? EntityContainerOperationStatus.InvalidParent // Cannot move to self or to a descendant of self.
                : EntityContainerOperationStatus.Success,
            (cont, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<EntityContainer>(cont, originalPath, parentKey);
                return new EntityContainerMovingNotification(moveEventInfo, eventMessages);
            },
            (cont, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<EntityContainer>(cont, originalPath, parentKey);
                return new EntityContainerMovedNotification(moveEventInfo, eventMessages);
            });

        // Only complete the scope when the move succeeded. A failure part way through rewriting the descendants
        // must roll back, or the subtree is left with a mix of old and new paths.
        if (moveResult.Success)
        {
            scope.Complete();
        }

        return moveResult;
    }

    /// <summary>
    ///     Moves a container, and everything below it, to a new parent, assuming the write lock has already been taken.
    /// </summary>
    /// <typeparam name="TNotification">
    ///     The type of the cancelable notification published before the move is performed.
    /// </typeparam>
    /// <param name="scope">The scope the move is running in. Note that this method never completes the scope.</param>
    /// <param name="key">The key of the container to move.</param>
    /// <param name="parentId">The ID of the node to move the container to, for example the tree root or the recycle bin.</param>
    /// <param name="parentPath">The path of the node identified by <paramref name="parentId" />, used to build the new paths.</param>
    /// <param name="parentLevel">The level of the node identified by <paramref name="parentId" />, used to calculate the level delta.</param>
    /// <param name="trash">Whether the container and its descendants are being moved to the recycle bin.</param>
    /// <param name="userKey">Key of the user issuing the move.</param>
    /// <param name="validateMove">
    ///     Performs any move validation that depends on the resolved container, for example rejecting a move into the
    ///     container's own descendants. Returning anything but
    ///     <see cref="EntityContainerOperationStatus.Success" /> aborts the move.
    /// </param>
    /// <param name="movingNotificationFactory">
    ///     Creates the cancelable notification published before the move. If a handler cancels it, the move is aborted
    ///     with <see cref="EntityContainerOperationStatus.CancelledByNotification" />.
    /// </param>
    /// <param name="movedNotificationFactory">
    ///     Creates the notification published after the move, with the state of the moving notification carried over.
    /// </param>
    /// <returns>An <see cref="Attempt{TStatus}" /> describing the outcome of the operation.</returns>
    /// <remarks>
    ///     The descendants are rewritten before the container itself is saved, as the rewrite slices the old container
    ///     path off each descendant path. The paging always requests page zero, because the query matching descendants
    ///     is based on the container path currently persisted - as descendants are rewritten they drop out of the
    ///     result set.
    /// </remarks>
    protected async Task<Attempt<EntityContainerOperationStatus>> MoveLockedAsync<TNotification>(
        ICoreScope scope,
        Guid key,
        int parentId,
        string parentPath,
        int parentLevel,
        bool trash,
        Guid userKey,
        Func<EntityContainer, EntityContainerOperationStatus> validateMove,
        Func<EntityContainer, EventMessages, TNotification> movingNotificationFactory,
        Func<EntityContainer, EventMessages, IStatefulNotification> movedNotificationFactory)
        where TNotification : IStatefulNotification, ICancelableNotification
    {
        EntityContainer? container = _entityContainerRepository.Get(key);
        if (container is null)
        {
            return Attempt.Fail(EntityContainerOperationStatus.NotFound);
        }

        // Capture original path before any modifications (needed for audit message when trashing).
        var originalPath = container.Path;

        if (container.ParentId == parentId)
        {
            return Attempt.Succeed(EntityContainerOperationStatus.Success);
        }

        EntityContainerOperationStatus validateMoveResult = validateMove(container);
        if (validateMoveResult != EntityContainerOperationStatus.Success)
        {
            return Attempt.Fail(validateMoveResult);
        }

        // The container repository throws when the new parent already has a container with this name, so check
        // up-front to report it as an operation status instead. This deliberately sits after the check for the
        // container already being at the requested parent, as the container would otherwise match itself.
        // Moves to the recycle bin are not checked: a name collision there should not stop a container being
        // trashed.
        if (trash is false && _entityContainerRepository.HasDuplicateName(parentId, container.Name!))
        {
            return Attempt.Fail(EntityContainerOperationStatus.DuplicateName);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        // Fire the moving notification and handle cancellation.
        TNotification movingNotification = movingNotificationFactory(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingNotification))
        {
            return Attempt.Fail(EntityContainerOperationStatus.CancelledByNotification);
        }

        var newContainerPath = $"{parentPath.TrimEnd(Constants.CharArrays.Comma)},{container.Id}";
        var levelDelta = 1 - container.Level + parentLevel;
        var movedEntities = new List<MoveEventInfo<TTreeEntity>>();
        UmbracoObjectTypes containedObjectType = ObjectTypes.GetUmbracoObjectType(ContainedObjectType);
        Guid containerObjectTypeId = ContainerObjectType.GetGuid();

        long total;

        do
        {
            IEnumerable<IEntitySlim> descendants = EntityService.GetPagedDescendants(
                container.Key,
                ContainerObjectType,
                [ContainerObjectType, containedObjectType],
                0, // pageIndex = 0 because the move operation is path based (starts-with), and we update paths as we move through the descendants
                DescendantsIteratorPageSize,
                out total);

            foreach (IEntitySlim descendant in descendants)
            {
                if (descendant.NodeObjectType == containerObjectTypeId)
                {
                    EntityContainer descendantContainer = _entityContainerRepository.Get(descendant.Id)
                                                          ?? throw new InvalidOperationException($"Descendant container with ID {descendant.Id} was not found.");
                    descendantContainer.Path = $"{newContainerPath}{descendant.Path[container.Path.Length..]}";
                    descendantContainer.Level += levelDelta;
                    descendantContainer.Trashed = trash;
                    _entityContainerRepository.Save(descendantContainer);
                }
                else
                {
                    TTreeEntity descendantEntity = GetContainedEntity(descendant.Id)
                                                   ?? throw new InvalidOperationException($"Descendant entity with ID {descendant.Id} was not found.");
                    var descendantOriginalPath = descendantEntity.Path;
                    descendantEntity.Path = $"{newContainerPath}{descendant.Path[container.Path.Length..]}";
                    descendantEntity.Level += levelDelta;

                    EntityContainerOperationStatus prepareStatus = await PrepareContainedEntityForMoveAsync(descendantEntity, trash, userKey);
                    if (prepareStatus != EntityContainerOperationStatus.Success)
                    {
                        return Attempt.Fail(prepareStatus);
                    }

                    SaveContainedEntity(descendantEntity);
                    movedEntities.Add(new MoveEventInfo<TTreeEntity>(descendantEntity, descendantOriginalPath, newParentKey: null));
                }
            }
        }
        while (total > DescendantsIteratorPageSize);

        // NOTE: as long as the parent ID is correct, the container repo takes care of updating the rest of the
        //       structural node data like path, level, sort orders etc.
        container.ParentId = parentId;
        container.Trashed = trash;

        _entityContainerRepository.Save(container);

        string? auditMessage = trash
            ? $"Moved to recycle bin from parent {originalPath.GetParentIdFromPath()}"
            : null;
        await AuditAsync(AuditType.Move, userKey, container.Id, auditMessage);

        PublishContainedEntitiesMovedNotifications(scope, movedEntities, eventMessages);

        // Fire the moved notification.
        IStatefulNotification movedNotification = movedNotificationFactory(container, eventMessages);
        scope.Notifications.Publish(movedNotification.WithStateFrom(movingNotification));

        return Attempt.Succeed(EntityContainerOperationStatus.Success);
    }

    private async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> SaveAsync(EntityContainer container, Guid userKey, Func<EntityContainerOperationStatus> operationValidation, AuditType auditType)
    {
        if (container.ContainedObjectType != ContainedObjectType)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.InvalidObjectType, container);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        WriteLock(scope);

        EntityContainerOperationStatus operationValidationStatus = operationValidation();
        if (operationValidationStatus != EntityContainerOperationStatus.Success)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(operationValidationStatus, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingEntityContainerNotification = new EntityContainerSavingNotification(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingEntityContainerNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.CancelledByNotification, container);
        }

        _entityContainerRepository.Save(container);

        await AuditAsync(auditType, userKey, container.Id);
        scope.Complete();

        scope.Notifications.Publish(new EntityContainerSavedNotification(container, eventMessages).WithStateFrom(savingEntityContainerNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
    }

    private EntityContainer? GetParent(ITreeEntity treeEntity)
    {
        if (treeEntity.ParentId == Constants.System.Root)
        {
            return null;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return _entityContainerRepository.Get(treeEntity.ParentId);
    }

    /// <summary>
    ///     Writes an audit entry for a container operation.
    /// </summary>
    /// <param name="type">The type of the audited operation.</param>
    /// <param name="userKey">Key of the user issuing the operation.</param>
    /// <param name="objectId">The ID of the container the operation was performed on.</param>
    /// <param name="comment">An optional comment describing the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task AuditAsync(AuditType type, Guid userKey, int objectId, string? comment = null) =>
        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            ContainerObjectType.GetName(),
            comment);

    /// <summary>
    ///     Takes the read locks required by this container type, if any.
    /// </summary>
    /// <param name="scope">The scope to take the locks on.</param>
    protected void ReadLock(ICoreScope scope)
    {
        if (ReadLockIds.Any())
        {
            scope.ReadLock(ReadLockIds);
        }
    }

    /// <summary>
    ///     Takes the write locks required by this container type, if any.
    /// </summary>
    /// <param name="scope">The scope to take the locks on.</param>
    protected void WriteLock(ICoreScope scope)
    {
        if (WriteLockIds.Any())
        {
            scope.WriteLock(WriteLockIds);
        }
    }
}
