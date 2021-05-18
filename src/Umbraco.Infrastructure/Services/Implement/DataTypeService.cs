using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
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
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ILocalizationService _localizationService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IJsonSerializer _jsonSerializer;

        public DataTypeService(
            IDataValueEditorFactory dataValueEditorFactory,
            IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository, IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository, IEntityRepository entityRepository, IContentTypeRepository contentTypeRepository,
            IIOHelper ioHelper, ILocalizedTextService localizedTextService, ILocalizationService localizationService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _dataValueEditorFactory = dataValueEditorFactory;
            _dataTypeRepository = dataTypeRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
            _contentTypeRepository = contentTypeRepository;
            _ioHelper = ioHelper;
            _localizedTextService = localizedTextService;
            _localizationService = localizationService;
            _shortStringHelper = shortStringHelper;
            _jsonSerializer = jsonSerializer;
        }

        #region Containers

        public Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    var container = new EntityContainer(Cms.Core.Constants.ObjectTypes.DataType)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
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

        public EntityContainer GetContainer(int containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeContainerRepository.Get(containerId);
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(name, level);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(IDataType dataType)
        {
            var ancestorIds = dataType.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    return asInt ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataType.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeContainerRepository.GetMany(containerIds);
            }
        }

        public Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != Cms.Core.Constants.ObjectTypes.DataType)
            {
                var ex = new InvalidOperationException("Not a " + Cms.Core.Constants.ObjectTypes.DataType + " container.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            using (var scope = ScopeProvider.CreateScope())
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

        public Attempt<OperationResult> DeleteContainer(int containerId, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                var container = _dataTypeContainerRepository.Get(containerId);
                if (container == null) return OperationResult.Attempt.NoOperation(evtMsgs);

                // 'container' here does not know about its children, so we need
                // to get it again from the entity repository, as a light entity
                var entity = _entityRepository.Get(container.Id);
                if (entity.HasChildren)
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

        public Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    var container = _dataTypeContainerRepository.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                        throw new InvalidOperationException("No container found with id " + id);

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
        public IDataType GetDataType(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var dataType = _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name)).FirstOrDefault();
                ConvertMissingEditorOfDataTypeToLabel(dataType);
                return dataType;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataType(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var dataType = _dataTypeRepository.Get(id);
                ConvertMissingEditorOfDataTypeToLabel(dataType);
                return dataType;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataType(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.Key == id);
                var dataType = _dataTypeRepository.Get(query).FirstOrDefault();
                ConvertMissingEditorOfDataTypeToLabel(dataType);
                return dataType;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        public IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
                var dataType = _dataTypeRepository.Get(query);
                ConvertMissingEditorsOfDataTypesToLabels(dataType);
                return dataType;
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAll(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var dataTypes = _dataTypeRepository.GetMany(ids);
                ConvertMissingEditorsOfDataTypesToLabels(dataTypes);
                return dataTypes;
            }
        }

        private void ConvertMissingEditorOfDataTypeToLabel(IDataType dataType)
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
            var dataTypesWithMissingEditors = dataTypes
                .Where(x => x.Editor is MissingPropertyEditor);
            foreach (var dataType in dataTypesWithMissingEditors)
            {
                dataType.Editor = new LabelPropertyEditor(_dataValueEditorFactory, _ioHelper);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moveInfo = new List<MoveEventInfo<IDataType>>();

            using (var scope = ScopeProvider.CreateScope())
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
                    EntityContainer container = null;
                    if (parentId > 0)
                    {
                        container = _dataTypeContainerRepository.Get(parentId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
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

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        public void Save(IDataType dataType, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            dataType.CreatorId = userId;

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataType>(dataType);

                var savingDataTypeNotification = new DataTypeSavingNotification(dataType, evtMsgs);
                if (scope.Notifications.PublishCancelable(savingDataTypeNotification))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataType.Name))
                {
                    throw new ArgumentException("Cannot save datatype with empty name.");
                }

                if (dataType.Name != null && dataType.Name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                _dataTypeRepository.Save(dataType);

                scope.Notifications.Publish(new DataTypeSavedNotification(dataType, evtMsgs).WithStateFrom(savingDataTypeNotification));

                Audit(AuditType.Save, userId, dataType.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            Save(dataTypeDefinitions, userId, true);
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId, bool raiseEvents)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var dataTypeDefinitionsA = dataTypeDefinitions.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var savingDataTypeNotification = new DataTypeSavingNotification(dataTypeDefinitions, evtMsgs);
                if (raiseEvents && scope.Notifications.PublishCancelable(savingDataTypeNotification))
                {
                    scope.Complete();
                    return;
                }

                foreach (var dataTypeDefinition in dataTypeDefinitionsA)
                {
                    dataTypeDefinition.CreatorId = userId;
                    _dataTypeRepository.Save(dataTypeDefinition);
                }

                if (raiseEvents)
                {
                    scope.Notifications.Publish(new DataTypeSavedNotification(dataTypeDefinitions, evtMsgs).WithStateFrom(savingDataTypeNotification));
                }
                Audit(AuditType.Save, userId, -1);

                scope.Complete();
            }
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
        public void Delete(IDataType dataType, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                var deletingDataTypeNotification = new DataTypeDeletingNotification(dataType, evtMsgs);
                if (scope.Notifications.PublishCancelable(deletingDataTypeNotification))
                {
                    scope.Complete();
                    return;
                }

                // find ContentTypes using this IDataTypeDefinition on a PropertyType, and delete
                // TODO: media and members?!
                // TODO: non-group properties?!
                var query = Query<PropertyType>().Where(x => x.DataTypeId == dataType.Id);
                var contentTypes = _contentTypeRepository.GetByQuery(query);
                foreach (var contentType in contentTypes)
                {
                    foreach (var propertyGroup in contentType.PropertyGroups)
                    {
                        var types = propertyGroup.PropertyTypes.Where(x => x.DataTypeId == dataType.Id).ToList();
                        foreach (var propertyType in types)
                        {
                            propertyGroup.PropertyTypes.Remove(propertyType);
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

                scope.Notifications.Publish(new DataTypeDeletedNotification(dataType, evtMsgs).WithStateFrom(deletingDataTypeNotification));

                Audit(AuditType.Delete, userId, dataType.Id);

                scope.Complete();
            }
        }

        public IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete:true))
            {
                return _dataTypeRepository.FindUsages(id);
            }
        }

        private void Audit(AuditType type, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.DataType)));
        }

    }
}
