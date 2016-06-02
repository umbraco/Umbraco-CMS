using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : RepositoryService, IDataTypeService
    {

        public DataTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

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

                    if (SavingContainer.IsRaisedEventCancelled(new SaveEventArgs<EntityContainer>(container, evtMsgs), this))
                        return OperationStatus.Attempt.Cancel(evtMsgs, container); // causes rollback

                    repo.AddOrUpdate(container);
                    uow.Complete();

                    SavedContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                var container = repo.Get(containerId);
                uow.Complete();
                return container;
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                var container = ((EntityContainerRepository)repo).Get(containerId);
                uow.Complete();
                return container;
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                var containers = ((EntityContainerRepository)repo).Get(name, level);
                uow.Complete();
                return containers;
            }
        }

        public IEnumerable<EntityContainer> GetContainers(IDataTypeDefinition dataTypeDefinition)
        {
            var ancestorIds = dataTypeDefinition.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    if (asInt) return asInt.Result;
                    return int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataTypeDefinition.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                var containers = repo.GetAll(containerIds);
                uow.Complete();
                return containers;
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

            if (SavingContainer.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
            {
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDataTypeContainerRepository>();
                repo.AddOrUpdate(container);
                uow.Complete();
            }

            SavedContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);

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

                if (DeletingContainer.IsRaisedEventCancelled(new DeleteEventArgs<EntityContainer>(container, evtMsgs), this))
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs)); // causes rollback

                repo.Delete(container);
                uow.Complete();

                DeletedContainer.RaiseEvent(new DeleteEventArgs<EntityContainer>(container, evtMsgs), this);

                return OperationStatus.Attempt.Succeed(evtMsgs);
                //TODO: Audit trail ?
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var def = repository.GetByQuery(repository.Query.Where(x => x.Name == name)).FirstOrDefault();
                uow.Complete();
                return def;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var def = repository.Get(id);
                uow.Complete();
                return def;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var query = repository.Query.Where(x => x.Key == id);
                var definition = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
                return definition;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var query = repository.Query.Where(x => x.PropertyEditorAlias == propertyEditorAlias);
                var definitions = repository.GetByQuery(query);
                uow.Complete();
                return definitions;
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        public IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var defs = repository.GetAll(ids);
                uow.Complete();
                return defs;
            }
        }

        /// <summary>
        /// Gets all prevalues for an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of string values</returns>
        public IEnumerable<string> GetPreValuesByDataTypeId(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var collection = repository.GetPreValuesCollectionByDataTypeId(id);
                //now convert the collection to a string list
                var list = collection.FormatAsDictionary()
                    .Select(x => x.Value.Value)
                    .ToList();
                uow.Complete();
                return list;
            }
        }

        /// <summary>
        /// Returns the PreValueCollection for the specified data type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PreValueCollection GetPreValuesCollectionByDataTypeId(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var vals = repository.GetPreValuesCollectionByDataTypeId(id);
                uow.Complete();
                return vals;
            }
        }

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                var val = repository.GetPreValueAsString(id);
                uow.Complete();
                return val;
            }
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> Move(IDataTypeDefinition toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (Moving.IsRaisedEventCancelled(
                  new MoveEventArgs<IDataTypeDefinition>(evtMsgs, new MoveEventInfo<IDataTypeDefinition>(toMove, toMove.Path, parentId)),
                  this))
            {
                return OperationStatus.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
            }

            var moveInfo = new List<MoveEventInfo<IDataTypeDefinition>>();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
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
                    uow.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    return OperationStatus.Attempt.Fail(ex.Operation, evtMsgs);
                }
            }

            Moved.RaiseEvent(new MoveEventArgs<IDataTypeDefinition>(false, evtMsgs, moveInfo.ToArray()), this);

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        /// <summary>
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataTypeDefinition dataTypeDefinition, int userId = 0)
        {
	        if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition), this))
				return;

            dataTypeDefinition.CreatorId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.AddOrUpdate(dataTypeDefinition);
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
            Audit(AuditType.Save, string.Format("Save DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
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
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitions), this))
                    return;
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                foreach (var dataTypeDefinition in dataTypeDefinitions)
                {
                    dataTypeDefinition.CreatorId = userId;
                    repository.AddOrUpdate(dataTypeDefinition);
                }
                uow.Complete();

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitions, false), this);
            }

            Audit(AuditType.Save, string.Format("Save DataTypeDefinition performed by user"), userId, -1);
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

                int sortOrder;
                if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out sortOrder) == false)
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
            var dtd = this.GetDataTypeDefinitionById(dataTypeId);
            if (dtd == null)
            {
                throw new InvalidOperationException("No data type found for id " + dataTypeId);
            }
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
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition), this))
                return;

            // if preValues contain the data type, override the data type definition accordingly
            if (values != null && values.ContainsKey(Constants.PropertyEditors.PreValueKeys.DataValueType))
                dataTypeDefinition.DatabaseType = PropertyValueEditor.GetDatabaseType(values[Constants.PropertyEditors.PreValueKeys.DataValueType].Value);

            dataTypeDefinition.CreatorId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.AddOrUpdate(dataTypeDefinition); // definition
                repository.AddOrUpdatePreValues(dataTypeDefinition, values); //prevalues
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
            Audit(AuditType.Save, "Save DataTypeDefinition performed by user", userId, dataTypeDefinition.Id);
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
	        if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition), this))
				return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IDataTypeDefinitionRepository>();
                repository.Delete(dataTypeDefinition);
		        uow.Complete();
	        }

            Deleted.RaiseEvent(new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);

            Audit(AuditType.Delete, string.Format("Delete DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
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