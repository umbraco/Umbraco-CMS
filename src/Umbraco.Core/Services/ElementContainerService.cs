using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementContainerService : EntityTypeContainerService<IElement, IElementContainerRepository>, IElementContainerService
{
    private readonly IElementContainerRepository _entityContainerRepository;
    private readonly IEntityService _entityService;
    private readonly IElementRepository _elementRepository;

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
        IElementRepository elementRepository)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
        _entityContainerRepository = entityContainerRepository;
        _entityService = entityService;
        _elementRepository = elementRepository;
    }

    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> MoveAsync(Guid key, Guid? parentKey, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        EntityContainer? container = _entityContainerRepository.Get(key);
        if (container is null)
        {
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.NotFound, null);
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

            if (parent.Path.StartsWith(container.Path))
            {
                // cannot move to descendant of self
                return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.InvalidParent, null);
            }

            parentId = parent.Id;
            parentPath = parent.Path;
            parentLevel = parent.Level;
        }

        if (container.ParentId == parentId)
        {
            return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        // fire the moving notification and handle cancellation
        var moveEventInfo = new MoveEventInfo<EntityContainer>(container, container.Path, parentId, parentKey);
        var movingEntityContainerNotification = new EntityContainerMovingNotification(moveEventInfo, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingEntityContainerNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.CancelledByNotification, container);
        }

        var newContainerPath = $"{parentPath},{container.Id}";
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
                    _entityContainerRepository.Save(descendantContainer);
                }
                else
                {
                    IElement descendantElement = _elementRepository.Get(descendant.Id)
                                                 ?? throw new InvalidOperationException($"Descendant element with ID {descendant.Id} was not found.");
                    descendantElement.Path = $"{newContainerPath}{descendant.Path[container.Path.Length..]}";
                    descendantElement.Level += levelDelta;
                    _elementRepository.Save(descendantElement);
                }
            }
        }
        while (total > DescendantsIteratorPageSize);

        // NOTE: as long as the parent ID is correct, the container repo takes care of updating the rest of the
        //       structural node data like path, level, sort orders etc.
        container.ParentId = parentId;

        _entityContainerRepository.Save(container);

        await AuditAsync(AuditType.Move, userKey, container.Id);

        // fire the moved notification
        scope.Notifications.Publish(new EntityContainerMovedNotification(moveEventInfo, eventMessages).WithStateFrom(movingEntityContainerNotification));

        scope.Complete();

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.Element;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.ElementContainer;

    protected override int[] ReadLockIds => new [] { Constants.Locks.ElementTree };

    protected override int[] WriteLockIds => ReadLockIds;
}
