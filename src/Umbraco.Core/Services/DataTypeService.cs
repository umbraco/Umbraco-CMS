using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    public class DataTypeService : RepositoryService, IDataTypeService
    {
        private readonly IDataValueEditorFactory _dataValueEditorFactory;
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        [Obsolete("Please use constructor that takes an ")]
        public DataTypeService(
            IDataValueEditorFactory dataValueEditorFactory,
            ICoreScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository,
            IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository,
            IEntityRepository entityRepository,
            IContentTypeRepository contentTypeRepository,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService,
            ILocalizationService localizationService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : this(
                dataValueEditorFactory,
                provider,
                loggerFactory,
                eventMessagesFactory,
                dataTypeRepository,
                dataTypeContainerRepository,
                auditRepository,
                entityRepository,
                contentTypeRepository,
                ioHelper,
                localizedTextService,
                localizationService,
                shortStringHelper,
                jsonSerializer,
                StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
            _dataValueEditorFactory = dataValueEditorFactory;
            _dataTypeRepository = dataTypeRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
            _contentTypeRepository = contentTypeRepository;
            _ioHelper = ioHelper;
        }

        public DataTypeService(
            IDataValueEditorFactory dataValueEditorFactory,
            ICoreScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository,
            IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository,
            IEntityRepository entityRepository,
            IContentTypeRepository contentTypeRepository,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService,
            ILocalizationService localizationService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IEditorConfigurationParser editorConfigurationParser)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _dataValueEditorFactory = dataValueEditorFactory;
            _dataTypeRepository = dataTypeRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
            _contentTypeRepository = contentTypeRepository;
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        #region Containers

        public Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DataType)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId,
                        Key = key
                    };

                    var savingEntityContainerNotification = new EntityContainerSavingNotification(container, evtMsgs);
                    if (scope.Notifications.PublishCancelable(savingEntityContainerNotification))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel(evtMsgs, container);
                    }

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    scope.Notifications.Publish(new EntityContainerSavedNotification(container, evtMsgs).WithStateFrom(savingEntityContainerNotification));

                    // TODO: Audit trail ?

                    return OperationResult.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        public EntityContainer? GetContainer(int containerId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.Get(containerId);
        }

        public EntityContainer? GetContainer(Guid containerId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.Get(containerId);
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.Get(name, level);
        }

        public IEnumerable<EntityContainer> GetContainers(IDataType dataType)
        {
            var ancestorIds = dataType.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    Attempt<int> asInt = x.TryConvertTo<int>();
                    return asInt.Success ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataType.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.GetMany(containerIds);
        }

        public Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != Constants.ObjectTypes.DataType)
            {
                var ex = new InvalidOperationException("Not a " + Constants.ObjectTypes.DataType + " container.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var savingEntityContainerNotification = new EntityContainerSavingNotification(container, evtMsgs);
                if (scope.Notifications.PublishCancelable(savingEntityContainerNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _dataTypeContainerRepository.Save(container);

                scope.Notifications.Publish(new EntityContainerSavedNotification(container, evtMsgs).WithStateFrom(savingEntityContainerNotification));
                scope.Complete();
            }

            // TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                EntityContainer? container = _dataTypeContainerRepository.Get(containerId);
                if (container == null)
                {
                    return OperationResult.Attempt.NoOperation(evtMsgs);
                }

                // 'container' here does not know about its children, so we need
                // to get it again from the entity repository, as a light entity
                IEntitySlim? entity = _entityRepository.Get(container.Id);
                if (entity?.HasChildren ?? false)
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, evtMsgs));
                }

                var deletingEntityContainerNotification = new EntityContainerDeletingNotification(container, evtMsgs);
                if (scope.Notifications.PublishCancelable(deletingEntityContainerNotification))
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, evtMsgs));
                }

                _dataTypeContainerRepository.Delete(container);

                scope.Notifications.Publish(new EntityContainerDeletedNotification(container, evtMsgs).WithStateFrom(deletingEntityContainerNotification));
                scope.Complete();
            }

            // TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                try
                {
                    EntityContainer? container = _dataTypeContainerRepository.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                    {
                        throw new InvalidOperationException("No container found with id " + id);
                    }

                    container.Name = name;

                    var renamingEntityContainerNotification = new EntityContainerRenamingNotification(container, evtMsgs);
                    if (scope.Notifications.PublishCancelable(renamingEntityContainerNotification))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel(evtMsgs, container);
                    }

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    scope.Notifications.Publish(new EntityContainerRenamedNotification(container, evtMsgs).WithStateFrom(renamingEntityContainerNotification));

                    return OperationResult.Attempt.Succeed(OperationResultType.Success, evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        [Obsolete("Please use GetAsync. Will be removed in V15.")]
        public IDataType? GetDataType(string name)
            => GetAsync(name).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<IDataType?> GetAsync(string name)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name))?.FirstOrDefault();
            ConvertMissingEditorOfDataTypeToLabel(dataType);
            return await Task.FromResult(dataType);
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        [Obsolete("Please use GetAsync. Will be removed in V15.")]
        public IDataType? GetDataType(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(id);
            ConvertMissingEditorOfDataTypeToLabel(dataType);
            return dataType;
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        [Obsolete("Please use GetAsync. Will be removed in V15.")]
        public IDataType? GetDataType(Guid id)
            => GetAsync(id).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<IDataType?> GetAsync(Guid id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = GetDataTypeFromRepository(id);
            ConvertMissingEditorOfDataTypeToLabel(dataType);
            return await Task.FromResult(dataType);
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        [Obsolete("Please use GetByEditorAliasAsync. Will be removed in V15.")]
        public IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias)
            => GetByEditorAliasAsync(propertyEditorAlias).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);
            return await Task.FromResult(dataTypes);
        }

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAll(params int[] ids)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.GetMany(ids).ToArray();

            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);
            return dataTypes;
        }

        private void ConvertMissingEditorOfDataTypeToLabel(IDataType? dataType)
        {
            if (dataType == null)
            {
                return;
            }

            ConvertMissingEditorsOfDataTypesToLabels(new[] { dataType });
        }

        private void ConvertMissingEditorsOfDataTypesToLabels(IEnumerable<IDataType> dataTypes)
        {
            // Any data types that don't have an associated editor are created of a specific type.
            // We convert them to labels to make clear to the user why the data type cannot be used.
            IEnumerable<IDataType> dataTypesWithMissingEditors = dataTypes
                .Where(x => x.Editor is MissingPropertyEditor);
            foreach (IDataType dataType in dataTypesWithMissingEditors)
            {
                dataType.Editor = new LabelPropertyEditor(_dataValueEditorFactory, _ioHelper, _editorConfigurationParser);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>?> Move(IDataType toMove, int parentId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            if (toMove.ParentId == parentId)
            {
                return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedNotAllowedByPath, evtMsgs);
            }

            var moveInfo = new List<MoveEventInfo<IDataType>>();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var moveEventInfo = new MoveEventInfo<IDataType>(toMove, toMove.Path, parentId);

                var movingDataTypeNotification = new DataTypeMovingNotification(moveEventInfo, evtMsgs);
                if (scope.Notifications.PublishCancelable(movingDataTypeNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
                }

                try
                {
                    EntityContainer? container = null;
                    if (parentId > 0)
                    {
                        container = _dataTypeContainerRepository.Get(parentId);
                        if (container == null)
                        {
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                        }
                    }
                    moveInfo.AddRange(_dataTypeRepository.Move(toMove, container));

                    scope.Notifications.Publish(new DataTypeMovedNotification(moveEventInfo, evtMsgs).WithStateFrom(movingDataTypeNotification));

                    scope.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    scope.Complete(); // TODO: what are we doing here exactly?
                    return OperationResult.Attempt.Fail(ex.Operation, evtMsgs);
                }
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        [Obsolete("Use the method which specifies the userId parameter")]
        public Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId)
        {
            return Copy(copying, containerId, Constants.Security.SuperUserId);
        }

        public Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            IDataType copy;
            try
            {
                if (containerId > 0)
                {
                    var container = GetContainer(containerId);
                    if (container is null)
                    {
                        throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                }
                copy = copying.DeepCloneWithResetIdentities();

                copy.Name += " (copy)"; // might not be unique
                copy.ParentId = containerId;

                Save(copy, userId);
            }
            catch (DataOperationException<MoveOperationStatusType> ex)
            {
                return OperationResult.Attempt.Fail<MoveOperationStatusType, IDataType>(ex.Operation, evtMsgs); // causes rollback
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, copy);
        }

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        public void Save(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            // mimic old service behavior
            if (string.IsNullOrWhiteSpace(dataType.Name))
            {
                throw new ArgumentException("Cannot save datatype with empty name.");
            }

            if (dataType.Name != null && dataType.Name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            Attempt<IDataType, DataTypeOperationStatus> result = SaveAsync(
                dataType,
                () => DataTypeOperationStatus.Success,
                userId,
                AuditType.Sort).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            if (dataType.Id != 0)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidId, dataType);
            }

            return await SaveAsync(dataType, () => DataTypeOperationStatus.Success, userId, AuditType.New);
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType, DataTypeOperationStatus>> UpdateAsync(IDataType dataType, int userId = Constants.Security.SuperUserId)
            => await SaveAsync(
                dataType,
                () =>
                {
                    IDataType? current = _dataTypeRepository.Get(dataType.Id);
                    return current == null
                        ? DataTypeOperationStatus.NotFound
                        : DataTypeOperationStatus.Success;
                },
                userId,
                AuditType.New);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        [Obsolete("Please use CreateAsync or UpdateAsync. Will be removed in V15.")]
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            IDataType[] dataTypeDefinitionsA = dataTypeDefinitions.ToArray();

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            var savingDataTypeNotification = new DataTypeSavingNotification(dataTypeDefinitions, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingDataTypeNotification))
            {
                scope.Complete();
                return;
            }

            foreach (IDataType dataTypeDefinition in dataTypeDefinitionsA)
            {
                dataTypeDefinition.CreatorId = userId;
                _dataTypeRepository.Save(dataTypeDefinition);
            }

            scope.Notifications.Publish(new DataTypeSavedNotification(dataTypeDefinitions, evtMsgs).WithStateFrom(savingDataTypeNotification));

            Audit(AuditType.Save, userId, -1);

            scope.Complete();
        }

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="IPropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Optional Id of the user issuing the deletion</param>
        public void Delete(IDataType dataType, int userId = Constants.Security.SuperUserId)
            => DeleteAsync(dataType.Key, userId).GetAwaiter().GetResult();

        /// <inheritdoc />
        public async Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            IDataType? dataType = GetDataTypeFromRepository(id);
            if (dataType == null)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.NotFound, dataType);
            }

            var deletingDataTypeNotification = new DataTypeDeletingNotification(dataType, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(deletingDataTypeNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.CancelledByNotification, dataType);
            }

            // find ContentTypes using this IDataTypeDefinition on a PropertyType, and delete
            // TODO: media and members?!
            // TODO: non-group properties?!
            IQuery<PropertyType> query = Query<PropertyType>().Where(x => x.DataTypeId == dataType.Id);
            IEnumerable<IContentType> contentTypes = _contentTypeRepository.GetByQuery(query);
            foreach (IContentType contentType in contentTypes)
            {
                foreach (PropertyGroup propertyGroup in contentType.PropertyGroups)
                {
                    var types = propertyGroup.PropertyTypes?.Where(x => x.DataTypeId == dataType.Id).ToList();
                    if (types is not null)
                    {
                        foreach (IPropertyType propertyType in types)
                        {
                            propertyGroup.PropertyTypes?.Remove(propertyType);
                        }
                    }
                }

                // so... we are modifying content types here. the service will trigger Deleted event,
                // which will propagate to DataTypeCacheRefresher which will clear almost every cache
                // there is to clear... and in addition published snapshot caches will clear themselves too, so
                // this is probably safe although it looks... weird.
                //
                // what IS weird is that a content type is losing a property and we do NOT raise any
                // content type event... so ppl better listen on the data type events too.

                _contentTypeRepository.Save(contentType);
            }

            _dataTypeRepository.Delete(dataType);

            scope.Notifications.Publish(new DataTypeDeletedNotification(dataType, eventMessages).WithStateFrom(deletingDataTypeNotification));

            Audit(AuditType.Delete, userId, dataType.Id);

            scope.Complete();

            return Attempt.SucceedWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType);
        }

        /// <inheritdoc />
        [Obsolete("Please use GetReferencesAsync. Will be deleted in V15.")]
        public IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete:true);
            return _dataTypeRepository.FindUsages(id);
        }

        /// <inheritdoc />
        public async Task<Attempt<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus>> GetReferencesAsync(Guid id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete:true);
            IDataType? dataType = GetDataTypeFromRepository(id);
            if (dataType == null)
            {
                return Attempt.FailWithStatus<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus>(DataTypeOperationStatus.NotFound, new Dictionary<Udi, IEnumerable<string>>());
            }

            IReadOnlyDictionary<Udi, IEnumerable<string>> usages = _dataTypeRepository.FindUsages(dataType.Id);
            return await Task.FromResult(Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, usages));
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType)
        {
            IConfigurationEditor? configurationEditor = dataType.Editor?.GetConfigurationEditor();
            return configurationEditor == null
                ? new[]
                {
                    new ValidationResult($"Data type with editor alias {dataType.EditorAlias} does not have a configuration editor")
                }
                : configurationEditor.Validate(dataType.ConfigurationData);
        }

        private async Task<Attempt<IDataType, DataTypeOperationStatus>> SaveAsync(
            IDataType dataType,
            Func<DataTypeOperationStatus> operationValidation,
            int userId,
            AuditType auditType)
        {
            IEnumerable<ValidationResult> validationResults = ValidateConfigurationData(dataType);
            if (validationResults.Any())
            {
                LoggerFactory
                    .CreateLogger<DataTypeService>()
                    .LogError($"Invalid data type configuration for data type: {dataType.Name} - validation error messages: {string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage))}");
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidConfiguration, dataType);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            dataType.CreatorId = userId;

            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            DataTypeOperationStatus status = operationValidation();
            if (status != DataTypeOperationStatus.Success)
            {
                return Attempt.FailWithStatus(status, dataType);
            }

            var savingDataTypeNotification = new DataTypeSavingNotification(dataType, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingDataTypeNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(DataTypeOperationStatus.CancelledByNotification, dataType);
            }

            if (string.IsNullOrWhiteSpace(dataType.Name))
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidName, dataType);
            }

            if (dataType.Name is { Length: > 255 })
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidName, dataType);
            }

            _dataTypeRepository.Save(dataType);

            scope.Notifications.Publish(new DataTypeSavedNotification(dataType, eventMessages).WithStateFrom(savingDataTypeNotification));

            Audit(auditType, userId, dataType.Id);
            scope.Complete();

            return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, dataType);
        }


        private IDataType? GetDataTypeFromRepository(Guid id)
        {
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.Key == id);
            IDataType? dataType = _dataTypeRepository.Get(query).FirstOrDefault();
            return dataType;
        }

        private void Audit(AuditType type, int userId, int objectId)
            => _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.DataType)));
    }
}
