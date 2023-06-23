using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class DataTypeContainerService : RepositoryService, IDataTypeContainerService
{
    private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public DataTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDataTypeContainerRepository dataTypeContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dataTypeContainerRepository = dataTypeContainerRepository;
        _auditRepository = auditRepository;
        _entityRepository = entityRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public async Task<EntityContainer?> GetAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await Task.FromResult(_dataTypeContainerRepository.Get(id));
    }

    /// <inheritdoc />
    public async Task<EntityContainer?> GetParentAsync(EntityContainer container)
        => await Task.FromResult(GetParent(container));

    /// <inheritdoc />
    public async Task<EntityContainer?> GetParentAsync(IDataType dataType)
        => await Task.FromResult(GetParent(dataType));

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> CreateAsync(EntityContainer container, Guid? parentKey, Guid userKey)
        => await SaveAsync(
            container,
            userKey,
            () =>
            {
                if (container.Id > 0)
                {
                    return DataTypeContainerOperationStatus.InvalidId;
                }

                if (_dataTypeContainerRepository.Get(container.Key) is not null)
                {
                    return DataTypeContainerOperationStatus.DuplicateKey;
                }

                EntityContainer? parentContainer = parentKey.HasValue
                    ? _dataTypeContainerRepository.Get(parentKey.Value)
                    : null;

                if (parentKey.HasValue && parentContainer == null)
                {
                    return DataTypeContainerOperationStatus.ParentNotFound;
                }



                if (_dataTypeContainerRepository.HasDuplicateName(container.ParentId, container.Name!))
                {
                    return DataTypeContainerOperationStatus.DuplicateName;
                }

                container.ParentId = parentContainer?.Id ?? Constants.System.Root;
                return DataTypeContainerOperationStatus.Success;
            },
            AuditType.New);

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> UpdateAsync(EntityContainer container, Guid userKey)
        => await SaveAsync(
            container,
            userKey,
            () =>
            {
                if (container.Id == 0)
                {
                    return DataTypeContainerOperationStatus.InvalidId;
                }

                if (container.IsPropertyDirty(nameof(EntityContainer.ParentId)))
                {
                    LoggerFactory.CreateLogger<DataTypeContainerService>().LogWarning($"Cannot use {nameof(UpdateAsync)} to change the container parent. Move the container instead.");
                    return DataTypeContainerOperationStatus.ParentNotFound;
                }

                return DataTypeContainerOperationStatus.Success;
            },
            AuditType.New);

    /// <inheritdoc />
    public async Task<Attempt<EntityContainer?, DataTypeContainerOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        EntityContainer? container = _dataTypeContainerRepository.Get(id);
        if (container == null)
        {
            return Attempt.FailWithStatus<EntityContainer?, DataTypeContainerOperationStatus>(DataTypeContainerOperationStatus.NotFound, null);
        }

        // 'container' here does not know about its children, so we need
        // to get it again from the entity repository, as a light entity
        IEntitySlim? entity = _entityRepository.Get(container.Id);
        if (entity?.HasChildren is true)
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, DataTypeContainerOperationStatus>(DataTypeContainerOperationStatus.NotEmpty, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingEntityContainerNotification = new EntityContainerDeletingNotification(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingEntityContainerNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<EntityContainer?, DataTypeContainerOperationStatus>(DataTypeContainerOperationStatus.CancelledByNotification, container);
        }

        _dataTypeContainerRepository.Delete(container);

        var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
        Audit(AuditType.Delete, currentUserId, container.Id);
        scope.Complete();

        scope.Notifications.Publish(new EntityContainerDeletedNotification(container, eventMessages).WithStateFrom(deletingEntityContainerNotification));

        return Attempt.SucceedWithStatus<EntityContainer?, DataTypeContainerOperationStatus>(DataTypeContainerOperationStatus.Success, container);
    }

    private async Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> SaveAsync(EntityContainer container, Guid userKey, Func<DataTypeContainerOperationStatus> operationValidation, AuditType auditType)
    {
        if (container.ContainedObjectType != Constants.ObjectTypes.DataType)
        {
            return Attempt.FailWithStatus(DataTypeContainerOperationStatus.InvalidObjectType, container);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        DataTypeContainerOperationStatus operationValidationStatus = operationValidation();
        if (operationValidationStatus != DataTypeContainerOperationStatus.Success)
        {
            return Attempt.FailWithStatus(operationValidationStatus, container);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingEntityContainerNotification = new EntityContainerSavingNotification(container, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingEntityContainerNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(DataTypeContainerOperationStatus.CancelledByNotification, container);
        }

        _dataTypeContainerRepository.Save(container);

        var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
        Audit(auditType, currentUserId, container.Id);
        scope.Complete();

        scope.Notifications.Publish(new EntityContainerSavedNotification(container, eventMessages).WithStateFrom(savingEntityContainerNotification));

        return Attempt.SucceedWithStatus(DataTypeContainerOperationStatus.Success, container);
    }

    private EntityContainer? GetParent(ITreeEntity treeEntity)
    {
        if (treeEntity.ParentId == Constants.System.Root)
        {
            return null;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _dataTypeContainerRepository.Get(treeEntity.ParentId);
    }

    private void Audit(AuditType type, int userId, int objectId)
        => _auditRepository.Save(new AuditItem(objectId, type, userId, UmbracoObjectTypes.DataTypeContainer.GetName()));
}
