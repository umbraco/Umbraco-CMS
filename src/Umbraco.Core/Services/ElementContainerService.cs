using System.Globalization;
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

internal sealed class ElementContainerService : EntityTypeContainerService<IElement, IElementContainerRepository>, IElementContainerService
{
    private readonly IElementContainerRepository _entityContainerRepository;
    private readonly IEntityService _entityService;
    private readonly IElementRepository _elementRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IElementService _elementService;
    private readonly ILogger<ElementContainerService> _logger;

    // internal so the tests can reach it
    internal const int DescendantsIteratorPageSize = 500;

    public ElementContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IElementContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityService entityService,
        IElementRepository elementRepository,
        IElementService elementService,
        ILogger<ElementContainerService> logger)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
        _entityContainerRepository = entityContainerRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _entityService = entityService;
        _elementRepository = elementRepository;
        _elementService = elementService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey);

    /// <inheritdoc/>
    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> RestoreAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleMoveAsync(key, parentKey, userKey, mustBeInRecycleBin: true);

    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        var originalPath = string.Empty;
        Attempt<EntityContainer?, EntityContainerOperationStatus> moveResult = await MoveLockedAsync(
            scope,
            key,
            Constants.System.RecycleBinElement,
            Constants.System.RecycleBinElementPathPrefix,
            0,
            true,
            userKey,
            _ => EntityContainerOperationStatus.Success,
            (container, eventMessages) =>
            {
                originalPath = container.Path;
                var moveEventInfo = new MoveToRecycleBinEventInfo<EntityContainer>(container, originalPath);
                return new EntityContainerMovingToRecycleBinNotification(moveEventInfo, eventMessages);
            },
            (container, eventMessages) =>
            {
                var moveEventInfo = new MoveToRecycleBinEventInfo<EntityContainer>(container, originalPath);
                return new EntityContainerMovedToRecycleBinNotification(moveEventInfo, eventMessages);
            });

        scope.Complete();
        return moveResult;
    }

    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        Attempt<EntityContainer?, EntityContainerOperationStatus> deleteResult = await DeleteLockedAsync(
            scope,
            key,
            userKey,
            true);

        scope.Complete();
        return deleteResult;
    }

    private async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> HandleMoveAsync(
        Guid key,
        Guid? parentKey,
        Guid userKey,
        bool mustBeInRecycleBin = false)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        EntityContainer? container = _entityContainerRepository.Get(key);
        if (container is null)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotFound, null);
        }

        if (mustBeInRecycleBin && container.Trashed is false)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotInTrash, container);
        }

        var parentId = Constants.System.Root;
        var parentPath = parentId.ToString();
        var parentLevel = 0;
        if (parentKey.HasValue && parentKey.Value != Guid.Empty)
        {
            EntityContainer? parent = _entityContainerRepository.Get(parentKey.Value);
            if (parent is null)
            {
                return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.ParentNotFound, null);
            }

            if (parent.Trashed)
            {
                // cannot move to a trashed container
                return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.InTrash, null);
            }

            parentId = parent.Id;
            parentPath = parent.Path;
            parentLevel = parent.Level;
        }

        Attempt<EntityContainer?, EntityContainerOperationStatus> moveResult = await MoveLockedAsync(
            scope,
            key,
            parentId,
            parentPath,
            parentLevel,
            false,
            userKey,
            cont => parentPath.StartsWith(cont.Path) ?
                EntityContainerOperationStatus.InvalidParent // cannot move to descendant of self
                : EntityContainerOperationStatus.Success,
            (cont, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<EntityContainer>(cont, cont.Path, parentId, parentKey);
                return new EntityContainerMovingNotification(moveEventInfo, eventMessages);
            },
            (cont, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<EntityContainer>(cont, cont.Path, parentId, parentKey);
                return new EntityContainerMovedNotification(moveEventInfo, eventMessages);
            });

        scope.Complete();
        return moveResult;
    }

    public async Task<Attempt<EntityContainerOperationStatus>> EmptyRecycleBinAsync(Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        EventMessages eventMessages = EventMessagesFactory.Get();

        // fire the emptying notification and handle cancellation
        var emptyingNotification = new ElementEmptyingRecycleBinNotification(eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(emptyingNotification))
        {
            return Attempt.Fail(EntityContainerOperationStatus.CancelledByNotification);
        }

        long total;
        do
        {
            IEnumerable<IEntitySlim> recycleBinRootItems = _entityService.GetPagedChildren(
                Constants.System.RecycleBinElementKey,
                [UmbracoObjectTypes.ElementContainer],
                [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element],
                0, // pageIndex = 0 because we continuously delete items as we move through the descendants
                DescendantsIteratorPageSize,
                trashed: true,
                out total);

            foreach (IEntitySlim recycleBinRootItem in recycleBinRootItems)
            {
                DeleteDescendantsLocked(recycleBinRootItem.Key);

                if (recycleBinRootItem.NodeObjectType == Constants.ObjectTypes.Element)
                {
                    IElement? element = _elementRepository.Get(recycleBinRootItem.Key);
                    if (element is not null)
                    {
                        _elementRepository.Delete(element);
                    }
                }
                else
                {
                    EntityContainer? container = await GetAsync(recycleBinRootItem.Key);
                    if (container is not null)
                    {
                        _entityContainerRepository.Delete(container);
                    }
                }
            }
        }
        while (total > DescendantsIteratorPageSize);

        await AuditAsync(AuditType.Delete, userKey, Constants.System.RecycleBinElement, "Recycle bin emptied");

        // fire the deleted notification
        scope.Notifications.Publish(new ElementEmptiedRecycleBinNotification(eventMessages).WithStateFrom(emptyingNotification));

        scope.Complete();
        return Attempt.Succeed(EntityContainerOperationStatus.Success);
    }

    private async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveLockedAsync<TNotification>(
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
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotFound, null);
        }

        // Capture original path before any modifications (needed for audit message when trashing)
        var originalPath = container.Path;

        if (container.ParentId == parentId)
        {
            return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
        }

        EntityContainerOperationStatus validateMoveResult = validateMove(container);
        if (validateMoveResult != EntityContainerOperationStatus.Success)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(validateMoveResult, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        // fire the moving notification and handle cancellation
        TNotification movingNotification = movingNotificationFactory(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingNotification))
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.CancelledByNotification, container);
        }

        var newContainerPath = $"{parentPath.TrimEnd(Constants.CharArrays.Comma)},{container.Id}";
        var levelDelta = 1 - container.Level + parentLevel;

        long total;

        do
        {
            IEnumerable<IEntitySlim> descendants = _entityService.GetPagedDescendants(
                container.Key,
                UmbracoObjectTypes.ElementContainer,
                [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element],
                0, // pageIndex = 0 because the move operation is path based (starts-with), and we update paths as we move through the descendants
                DescendantsIteratorPageSize,
                out total);

            foreach (IEntitySlim descendant in descendants)
            {
                if (descendant.NodeObjectType == Constants.ObjectTypes.ElementContainer)
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
                    IElement descendantElement = _elementRepository.Get(descendant.Id)
                                                 ?? throw new InvalidOperationException($"Descendant element with ID {descendant.Id} was not found.");
                    descendantElement.Path = $"{newContainerPath}{descendant.Path[container.Path.Length..]}";
                    descendantElement.Level += levelDelta;

                    // make sure the element is unpublished if it is moved from trash
                    var unpublishSuccess = await ElementEditingService.UnpublishTrashedElementOnRestore(descendantElement, userKey, _elementService, _userIdKeyResolver, _logger);
                    if (unpublishSuccess is false)
                    {
                        return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Unknown, container);
                    }

                    // NOTE: this cast isn't pretty, but it's the best we can do now. the content and media services do something
                    //       similar, and at the time of writing this, we are subject to the limitations imposed there.
                    ((TreeEntityBase)descendantElement).Trashed = trash;
                    _elementRepository.Save(descendantElement);
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

        // fire the moved notification
        IStatefulNotification movedNotification = movedNotificationFactory(container, eventMessages);
        scope.Notifications.Publish(movedNotification.WithStateFrom(movingNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
    }

    private async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteLockedAsync(
        ICoreScope scope,
        Guid key,
        Guid userKey,
        bool mustBeTrashed)
    {
        EntityContainer? container = _entityContainerRepository.Get(key);
        if (container is null)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotFound, null);
        }

        if (mustBeTrashed && container.Trashed is false)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotInTrash, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        // fire the deleting notification and handle cancellation
        var deletingNotification = new EntityContainerDeletingNotification(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.CancelledByNotification, container);
        }

        DeleteDescendantsLocked(container.Key);

        _entityContainerRepository.Delete(container);

        await AuditAsync(AuditType.Delete, userKey, container.Id);

        // fire the deleted notification
        scope.Notifications.Publish(new EntityContainerDeletedNotification(container, eventMessages).WithStateFrom(deletingNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
    }

    private void DeleteDescendantsLocked(Guid key)
    {
        long total;

        do
        {
            IEnumerable<IEntitySlim> descendants = _entityService.GetPagedDescendants(
                key,
                UmbracoObjectTypes.ElementContainer,
                [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element],
                0, // pageIndex = 0 because we continuously delete items as we move through the descendants
                DescendantsIteratorPageSize,
                out total);

            foreach (IEntitySlim descendant in descendants)
            {
                if (descendant.NodeObjectType == Constants.ObjectTypes.ElementContainer)
                {
                    EntityContainer descendantContainer = _entityContainerRepository.Get(descendant.Id)
                                                          ?? throw new InvalidOperationException($"Descendant container with ID {descendant.Id} was not found.");
                    _entityContainerRepository.Delete(descendantContainer);
                }
                else
                {
                    IElement descendantElement = _elementRepository.Get(descendant.Id)
                                                 ?? throw new InvalidOperationException($"Descendant element with ID {descendant.Id} was not found.");
                    _elementRepository.Delete(descendantElement);
                }
            }
        }
        while (total > DescendantsIteratorPageSize);
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.Element;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.ElementContainer;

    protected override int[] ReadLockIds => new [] { Constants.Locks.ElementTree };

    protected override int[] WriteLockIds => ReadLockIds;
}
