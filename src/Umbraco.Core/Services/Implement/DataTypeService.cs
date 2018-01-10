using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    internal class DataTypeService : ScopeRepositoryService, IDataTypeService
    {
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;

        public DataTypeService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository, IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository, IEntityRepository entityRepository, IContentTypeRepository contentTypeRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _dataTypeRepository = dataTypeRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
            _contentTypeRepository = contentTypeRepository;
        }

        #region Containers

        public Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DataType)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (scope.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel(evtMsgs, container);
                    }

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                    //TODO: Audit trail ?

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
            var ancestorIds = dataType.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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

        public Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();

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

            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _dataTypeContainerRepository.Save(container);

                scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                scope.Complete();
            }

            //TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult> DeleteContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                var container = _dataTypeContainerRepository.Get(containerId);
                if (container == null) return OperationResult.Attempt.NoOperation(evtMsgs);

                var entity = _entityRepository.Get(container.Id);
                if (entity.HasChildren()) // because container.HasChildren() does not work?
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, evtMsgs)); // causes rollback

                if (scope.Events.DispatchCancelable(DeletingContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, evtMsgs));
                }

                _dataTypeContainerRepository.Delete(container);

                scope.Events.Dispatch(DeletedContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs));
                scope.Complete();
            }

            //TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = 0)
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

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    // fixme - triggering SavedContainer with a different name?!
                    scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs), "RenamedContainer");

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
        public IDataType GetDataTypeDefinitionByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataTypeDefinitionById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataTypeDefinitionById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.Key == id);
                return _dataTypeRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataType> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
                return _dataTypeRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAllDataTypeDefinitions(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Gets all prevalues for an <see cref="IDataType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of string values</returns>
        public IEnumerable<string> GetPreValuesByDataTypeId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var collection = _dataTypeRepository.GetPreValuesCollectionByDataTypeId(id);
                //now convert the collection to a string list
                return collection.FormatAsDictionary()
                    .Select(x => x.Value.Value)
                    .ToList();
            }
        }

        /// <summary>
        /// Returns the PreValueCollection for the specified data type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PreValueCollection GetPreValuesCollectionByDataTypeId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.GetPreValuesCollectionByDataTypeId(id);
            }
        }

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.GetPreValueAsString(id);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moveInfo = new List<MoveEventInfo<IDataType>>();

            using (var scope = ScopeProvider.CreateScope())
            {
                var moveEventInfo = new MoveEventInfo<IDataType>(toMove, toMove.Path, parentId);
                var moveEventArgs = new MoveEventArgs<IDataType>(evtMsgs, moveEventInfo);
                if (scope.Events.DispatchCancelable(Moving, this, moveEventArgs))
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

                    moveEventArgs.MoveInfoCollection = moveInfo;
                    moveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Moved, this, moveEventArgs);
                    scope.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    scope.Complete(); // fixme what are we doing here exactly?
                    return OperationResult.Attempt.Fail(ex.Operation, evtMsgs);
                }
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataType dataType, int userId = 0)
        {
            dataType.CreatorId = userId;

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataType>(dataType);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataType.Name))
                {
                    throw new ArgumentException("Cannot save datatype with empty name.");
                }

                _dataTypeRepository.Save(dataType);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataType.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = 0)
        {
            Save(dataTypeDefinitions, userId, true);
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId, bool raiseEvents)
        {
            var dataTypeDefinitionsA = dataTypeDefinitions.ToArray();
            var saveEventArgs = new SaveEventArgs<IDataType>(dataTypeDefinitionsA);

            using (var scope = ScopeProvider.CreateScope())
            {
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
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
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
                Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, -1);

                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="dataTypeId">Id of the DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of string values to save</param>
        [Obsolete("This should no longer be used, use the alternative SavePreValues or SaveDataTypeAndPreValues methods instead. This will only insert pre-values without keys")]
        public void SavePreValues(int dataTypeId, IEnumerable<string> values)
        {
            //TODO: Should we raise an event here since we are really saving values for the data type?

            using (var scope = ScopeProvider.CreateScope())
            {
                var sortOrderObj = scope.Database.ExecuteScalar<object>(
                    "SELECT max(sortorder) FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId", new { DataTypeId = dataTypeId });

                if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out int sortOrder) == false)
                    sortOrder = 1;

                foreach (var value in values)
                {
                    var dto = new DataTypePreValueDto { DataTypeNodeId = dataTypeId, Value = value, SortOrder = sortOrder };
                    scope.Database.Insert(dto);
                    sortOrder++;
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Saves/updates the pre-values
        /// </summary>
        /// <param name="dataTypeId"></param>
        /// <param name="values"></param>
        /// <remarks>
        /// We need to actually look up each pre-value and maintain it's id if possible - this is because of silly property editors
        /// like 'dropdown list publishing keys'
        /// </remarks>
        public void SavePreValues(int dataTypeId, IDictionary<string, PreValue> values)
        {
            var dtd = GetDataTypeDefinitionById(dataTypeId);
            if (dtd == null)
                throw new InvalidOperationException("No data type found for id " + dataTypeId);

            SavePreValues(dtd, values);
        }

        /// <summary>
        /// Saves/updates the pre-values
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="values"></param>
        /// <remarks>
        /// We need to actually look up each pre-value and maintain it's id if possible - this is because of silly property editors
        /// like 'dropdown list publishing keys'
        /// </remarks>
        public void SavePreValues(IDataType dataType, IDictionary<string, PreValue> values)
        {
            //TODO: Should we raise an event here since we are really saving values for the data type?

            using (var scope = ScopeProvider.CreateScope())
            {
                _dataTypeRepository.AddOrUpdatePreValues(dataType, values);
                scope.Complete();
            }
        }

        /// <summary>
        /// This will save a data type and it's pre-values in one transaction
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="values"></param>
        /// <param name="userId"></param>
        public void SaveDataTypeAndPreValues(IDataType dataType, IDictionary<string, PreValue> values, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataType>(dataType);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                // if preValues contain the data type, override the data type definition accordingly
                if (values != null && values.ContainsKey(Constants.PropertyEditors.PreValueKeys.DataValueType))
                    dataType.DatabaseType = PropertyValueEditor.GetDatabaseType(values[Constants.PropertyEditors.PreValueKeys.DataValueType].Value);

                dataType.CreatorId = userId;

                _dataTypeRepository.Save(dataType); // definition
                _dataTypeRepository.AddOrUpdatePreValues(dataType, values); //prevalues

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataType.Id);

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Optional Id of the user issueing the deletion</param>
        public void Delete(IDataType dataType, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IDataType>(dataType);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }


                // find ContentTypes using this IDataTypeDefinition on a PropertyType, and delete
                // fixme - media and members?!
                // fixme - non-group properties?!
                var query = Query<PropertyType>().Where(x => x.DataTypeDefinitionId == dataType.Id);
                var contentTypes = _contentTypeRepository.GetByQuery(query);
                foreach (var contentType in contentTypes)
                {
                    foreach (var propertyGroup in contentType.PropertyGroups)
                    {
                        var types = propertyGroup.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataType.Id).ToList();
                        foreach (var propertyType in types)
                        {
                            propertyGroup.PropertyTypes.Remove(propertyType);
                        }
                    }

                    // so... we are modifying content types here. the service will trigger Deleted event,
                    // which will propagate to DataTypeCacheRefresher which will clear almost every cache
                    // there is to clear... and in addition published snapshot caches will clear themselves too, so
                    // this is probably safe alghough it looks... weird.
                    //
                    // what IS weird is that a content type is losing a property and we do NOT raise any
                    // content type event... so ppl better listen on the data type events too.

                    _contentTypeRepository.Save(contentType);
                }

                _dataTypeRepository.Delete(dataType);

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
                Audit(AuditType.Delete, "Delete DataTypeDefinition performed by user", userId, dataType.Id);

                scope.Complete();
            }
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, message, type, userId));
        }

        #region Event Handlers

        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<EntityContainer>> SavingContainer;
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<EntityContainer>> SavedContainer;
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<EntityContainer>> DeletingContainer;
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<EntityContainer>> DeletedContainer;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataType>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataType>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataType>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataType>> Saved;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataType>> Moving;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataType>> Moved;
        #endregion
    }
}
