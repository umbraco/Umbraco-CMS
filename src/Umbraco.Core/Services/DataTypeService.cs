using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/>
    /// </summary>
    internal class DataTypeService : ScopeRepositoryService, IDataTypeService
    {
        private readonly IDataTypeDefinitionRepository _dataTypeDefinitionRepository;
        private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;

        public DataTypeService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IDataTypeDefinitionRepository dataTypeDefinitionRepository, IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository, IEntityRepository entityRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _dataTypeDefinitionRepository = dataTypeDefinitionRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeContainerRepository.Get(containerId);
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(name, level);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(IDataTypeDefinition dataTypeDefinition)
        {
            var ancestorIds = dataTypeDefinition.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    return asInt ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataTypeDefinition.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
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
        /// Gets a <see cref="IDataTypeDefinition"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeDefinitionRepository.Get(Query<IDataTypeDefinition>().Where(x => x.Name == name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeDefinitionRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IDataTypeDefinition>().Where(x => x.Key == id);
                return _dataTypeDefinitionRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IDataTypeDefinition>().Where(x => x.PropertyEditorAlias == propertyEditorAlias);
                return _dataTypeDefinitionRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        public IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeDefinitionRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Gets all prevalues for an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of string values</returns>
        public IEnumerable<string> GetPreValuesByDataTypeId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var collection = _dataTypeDefinitionRepository.GetPreValuesCollectionByDataTypeId(id);
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeDefinitionRepository.GetPreValuesCollectionByDataTypeId(id);
            }
        }

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _dataTypeDefinitionRepository.GetPreValueAsString(id);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>> Move(IDataTypeDefinition toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moveInfo = new List<MoveEventInfo<IDataTypeDefinition>>();

            using (var scope = ScopeProvider.CreateScope())
            {
                var moveEventInfo = new MoveEventInfo<IDataTypeDefinition>(toMove, toMove.Path, parentId);
                var moveEventArgs = new MoveEventArgs<IDataTypeDefinition>(evtMsgs, moveEventInfo);
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
                    moveInfo.AddRange(_dataTypeDefinitionRepository.Move(toMove, container));

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
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataTypeDefinition dataTypeDefinition, int userId = 0)
        {
            dataTypeDefinition.CreatorId = userId;

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataTypeDefinition.Name))
                {
                    throw new ArgumentException("Cannot save datatype with empty name.");
                }

                _dataTypeDefinitionRepository.Save(dataTypeDefinition);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, int userId = 0)
        {
            Save(dataTypeDefinitions, userId, true);
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        public void Save(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, int userId, bool raiseEvents)
        {
            var dataTypeDefinitionsA = dataTypeDefinitions.ToArray();
            var saveEventArgs = new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitionsA);

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
                    _dataTypeDefinitionRepository.Save(dataTypeDefinition);
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
        /// <param name="dataTypeDefinition"></param>
        /// <param name="values"></param>
        /// <remarks>
        /// We need to actually look up each pre-value and maintain it's id if possible - this is because of silly property editors
        /// like 'dropdown list publishing keys'
        /// </remarks>
        public void SavePreValues(IDataTypeDefinition dataTypeDefinition, IDictionary<string, PreValue> values)
        {
            //TODO: Should we raise an event here since we are really saving values for the data type?

            using (var scope = ScopeProvider.CreateScope())
            {
                _dataTypeDefinitionRepository.AddOrUpdatePreValues(dataTypeDefinition, values);
                scope.Complete();
            }
        }

        /// <summary>
        /// This will save a data type and it's pre-values in one transaction
        /// </summary>
        /// <param name="dataTypeDefinition"></param>
        /// <param name="values"></param>
        /// <param name="userId"></param>
        public void SaveDataTypeAndPreValues(IDataTypeDefinition dataTypeDefinition, IDictionary<string, PreValue> values, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                // if preValues contain the data type, override the data type definition accordingly
                if (values != null && values.ContainsKey(Constants.PropertyEditors.PreValueKeys.DataValueType))
                    dataTypeDefinition.DatabaseType = PropertyValueEditor.GetDatabaseType(values[Constants.PropertyEditors.PreValueKeys.DataValueType].Value);

                dataTypeDefinition.CreatorId = userId;

                _dataTypeDefinitionRepository.Save(dataTypeDefinition); // definition
                _dataTypeDefinitionRepository.AddOrUpdatePreValues(dataTypeDefinition, values); //prevalues

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataTypeDefinition"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataTypeDefinition"/>.
        /// </remarks>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to delete</param>
        /// <param name="userId">Optional Id of the user issueing the deletion</param>
        public void Delete(IDataTypeDefinition dataTypeDefinition, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _dataTypeDefinitionRepository.Delete(dataTypeDefinition);

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
                Audit(AuditType.Delete, "Delete DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);

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
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataTypeDefinition>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataTypeDefinition>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataTypeDefinition>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataTypeDefinition>> Saved;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataTypeDefinition>> Moving;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataTypeDefinition>> Moved;
        #endregion
    }
}
