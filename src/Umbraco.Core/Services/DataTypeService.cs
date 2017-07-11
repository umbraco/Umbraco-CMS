using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : ScopeRepositoryService, IDataTypeService
    {
        public DataTypeService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }

        #region Containers

        public Attempt<OperationStatus<OperationStatusType, EntityContainer>> CreateContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DataTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (uow.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                    {
                        uow.Complete();
                        return OperationStatus.Attempt.Cancel(evtMsgs, container);
                    }

                    repo.AddOrUpdate(container);
                    uow.Complete();

                    uow.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                    //TODO: Audit trail ?

                    return OperationStatus.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationStatus.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        public EntityContainer GetContainer(int containerId)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                return repo.Get(containerId);
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                return ((EntityContainerRepository)repo).Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                return ((EntityContainerRepository)repo).Get(name, level);
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                return repo.GetAll(containerIds);
            }
        }

        public Attempt<OperationStatus> SaveContainer(EntityContainer container, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != Constants.ObjectTypes.DataTypeGuid)
            {
                var ex = new InvalidOperationException("Not a " + Constants.ObjectTypes.DataTypeGuid + " container.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                repo.AddOrUpdate(container);

                uow.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                uow.Complete();
            }

            //TODO: Audit trail ?
            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationStatus> DeleteContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                var container = repo.Get(containerId);
                if (container == null) return OperationStatus.Attempt.NoOperation(evtMsgs);

                var erepo = uow.CreateRepository<IEntityRepository>();
                var entity = erepo.Get(container.Id);
                if (entity.HasChildren()) // because container.HasChildren() does not work?
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCannot, evtMsgs)); // causes rollback

                if (uow.Events.DispatchCancelable(DeletingContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    uow.Complete();
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);

                uow.Events.Dispatch(DeletedContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs));
                uow.Complete();
            }

            //TODO: Audit trail ?
            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        #endregion

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionByName(string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                return repository.GetByQuery(uow.Query<IDataTypeDefinition>().Where(x => x.Name == name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var query = uow.Query<IDataTypeDefinition>().Where(x => x.Key == id);
                return repository.GetByQuery(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var query = uow.Query<IDataTypeDefinition>().Where(x => x.PropertyEditorAlias == propertyEditorAlias);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        public IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets all prevalues for an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of string values</returns>
        public IEnumerable<string> GetPreValuesByDataTypeId(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var collection = repository.GetPreValuesCollectionByDataTypeId(id);
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                return repository.GetPreValuesCollectionByDataTypeId(id);
            }
        }

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                return repository.GetPreValueAsString(id);
            }
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> Move(IDataTypeDefinition toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moveInfo = new List<MoveEventInfo<IDataTypeDefinition>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Moving, this, new MoveEventArgs<IDataTypeDefinition>(evtMsgs, new MoveEventInfo<IDataTypeDefinition>(toMove, toMove.Path, parentId))))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
                }

                var containerRepository = uow.CreateRepository<IDataTypeContainerRepository>();
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();

                try
                {
                    EntityContainer container = null;
                    if (parentId > 0)
                    {
                        container = containerRepository.Get(parentId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    moveInfo.AddRange(repository.Move(toMove, container));

                    uow.Events.Dispatch(Moved, this, new MoveEventArgs<IDataTypeDefinition>(false, evtMsgs, moveInfo.ToArray()));
                    uow.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    uow.Complete(); // fixme what are we doing here exactly?
                    return OperationStatus.Attempt.Fail(ex.Operation, evtMsgs);
                }
            }

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        /// <summary>
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataTypeDefinition dataTypeDefinition, int userId = 0)
        {
            dataTypeDefinition.CreatorId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition)))
                {
                    uow.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataTypeDefinition.Name))
                {
                    throw new ArgumentException("Cannot save datatype with empty name.");
                }

                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.AddOrUpdate(dataTypeDefinition);

                uow.Events.Dispatch(Saved, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false));
                Audit(uow, AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);
                uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitionsA)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                foreach (var dataTypeDefinition in dataTypeDefinitionsA)
                {
                    dataTypeDefinition.CreatorId = userId;
                    repository.AddOrUpdate(dataTypeDefinition);
                }

                if (raiseEvents)
                    uow.Events.Dispatch(Saved, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitionsA, false));
                Audit(uow, AuditType.Save, "Save DataTypeDefinition performed by user", userId, -1);

                uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var sortOrderObj = uow.Database.ExecuteScalar<object>(
                    "SELECT max(sortorder) FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId", new { DataTypeId = dataTypeId });

                if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out int sortOrder) == false)
                    sortOrder = 1;

                foreach (var value in values)
                {
                    var dto = new DataTypePreValueDto { DataTypeNodeId = dataTypeId, Value = value, SortOrder = sortOrder };
                    uow.Database.Insert(dto);
                    sortOrder++;
                }

                uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.AddOrUpdatePreValues(dataTypeDefinition, values);
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition)))
                {
                    uow.Complete();
                    return;
                }

                // if preValues contain the data type, override the data type definition accordingly
                if (values != null && values.ContainsKey(Constants.PropertyEditors.PreValueKeys.DataValueType))
                    dataTypeDefinition.DatabaseType = PropertyValueEditor.GetDatabaseType(values[Constants.PropertyEditors.PreValueKeys.DataValueType].Value);

                dataTypeDefinition.CreatorId = userId;

                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.AddOrUpdate(dataTypeDefinition); // definition
                repository.AddOrUpdatePreValues(dataTypeDefinition, values); //prevalues

                uow.Events.Dispatch(Saved, this, new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false));
                Audit(uow, AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);

                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.Delete(dataTypeDefinition);

                uow.Events.Dispatch(Deleted, this, new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition, false));
                Audit(uow, AuditType.Delete, "Delete DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);

                uow.Complete();
	        }
        }

        private void Audit(IUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repo = uow.CreateRepository<IAuditRepository>();
            repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
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