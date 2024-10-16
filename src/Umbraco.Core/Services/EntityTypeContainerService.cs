using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal abstract class EntityTypeContainerService<TTreeEntity, TEntityContainerRepository> : RepositoryService, IEntityTypeContainerService<TTreeEntity>
    where TTreeEntity : ITreeEntity
    where TEntityContainerRepository : IEntityContainerRepository
{
    private readonly TEntityContainerRepository _entityContainerRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    protected abstract Guid ContainedObjectType { get; }

    protected abstract UmbracoObjectTypes ContainerObjectType { get; }

    protected abstract int[] ReadLockIds { get; }

    protected abstract int[] WriteLockIds { get; }

    protected EntityTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TEntityContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _entityContainerRepository = entityContainerRepository;
        _auditRepository = auditRepository;
        _entityRepository = entityRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<EntityContainer?> GetAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return await Task.FromResult(_entityContainerRepository.Get(id));
    }


    /// <inheritdoc />
    public async Task<IEnumerable<EntityContainer>> GetAsync(string name, int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return await Task.FromResult(_entityContainerRepository.Get(name, level));
    }
    /// <inheritdoc />
    public async Task<IEnumerable<EntityContainer>> GetAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ReadLock(scope);
        return await Task.FromResult(_entityContainerRepository.GetMany());
    }

    /// <inheritdoc />
    public async Task<EntityContainer?> GetParentAsync(EntityContainer container)
        => await Task.FromResult(GetParent(container));

    /// <inheritdoc />
    public async Task<EntityContainer?> GetParentAsync(TTreeEntity entity)
        => await Task.FromResult(GetParent(entity));

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

                if (_entityContainerRepository.HasDuplicateName(container.ParentId, container.Name!))
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
                    return EntityContainerOperationStatus.ParentNotFound;
                }

                return EntityContainerOperationStatus.Success;
            },
            AuditType.New);
    }

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteAsync(Guid id, Guid userKey)
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

        var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
        Audit(AuditType.Delete, currentUserId, container.Id);
        scope.Complete();

        scope.Notifications.Publish(new EntityContainerDeletedNotification(container, eventMessages).WithStateFrom(deletingEntityContainerNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, EntityContainerOperationStatus>(EntityContainerOperationStatus.Success, container);
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

        var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
        Audit(auditType, currentUserId, container.Id);
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

    private void Audit(AuditType type, int userId, int objectId)
        => _auditRepository.Save(new AuditItem(objectId, type, userId, ContainerObjectType.GetName()));

    private void ReadLock(ICoreScope scope)
    {
        if (ReadLockIds.Any())
        {
            scope.ReadLock(ReadLockIds);
        }
    }

    private void WriteLock(ICoreScope scope)
    {
        if (WriteLockIds.Any())
        {
            scope.WriteLock(WriteLockIds);
        }
    }
}
