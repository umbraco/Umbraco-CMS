using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using umbraco.interfaces;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : RepositoryService, IDataTypeService
    {

        public DataTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
        }

        #region Containers

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> CreateContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DataTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (SavingContainer.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                    {
                        return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.FailedCancelledByEvent, evtMsgs));
                    }

                    repo.AddOrUpdate(container);
                    uow.Commit();

                    SavedContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);
                    //TODO: Audit trail ?

                    return Attempt.Succeed(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.Success, evtMsgs));
                }
                catch (Exception ex)
                {
                    return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(null, OperationStatusType.FailedExceptionThrown, evtMsgs), ex);
                }
            }
        }

        public EntityContainer GetContainer(int containerId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                var container = repo.Get(containerId);
                return container;
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                var container = repo.Get(containerId);
                return container;
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                return repo.Get(name, level);
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                return repo.GetAll(containerIds);
            }
        }

        public Attempt<OperationStatus> SaveContainer(EntityContainer container, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != Constants.ObjectTypes.DataTypeGuid)
            {
                var ex = new InvalidOperationException("Not a " + Constants.ObjectTypes.DataTypeGuid + " container.");
                return OperationStatus.Exception(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationStatus.Exception(evtMsgs, ex);
            }

            if (SavingContainer.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
            {
                return OperationStatus.Cancelled(evtMsgs);
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                repo.AddOrUpdate(container);
                uow.Commit();
            }

            SavedContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);

            //TODO: Audit trail ?

            return OperationStatus.Success(evtMsgs);
        }

        public Attempt<OperationStatus> DeleteContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            {
                var container = repo.Get(containerId);
                if (container == null) return OperationStatus.NoOperation(evtMsgs);

                if (DeletingContainer.IsRaisedEventCancelled(
                        new DeleteEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                {
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);
                uow.Commit();

                DeletedContainer.RaiseEvent(new DeleteEventArgs<EntityContainer>(container, evtMsgs), this);

                return OperationStatus.Success(evtMsgs);
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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetByQuery(new Query<IDataTypeDefinition>().Where(x => x.Name == name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IDataTypeDefinition>.Builder.Where(x => x.Key == id);
                var definitions = repository.GetByQuery(query);

                return definitions.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="id">Id of the DataType control</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the overload GetDataTypeDefinitionByPropertyEditorAlias instead")]
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByControlId(Guid id)
        {
            var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(id, true);
            return GetDataTypeDefinitionByPropertyEditorAlias(alias);
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias)
        {
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IDataTypeDefinition>.Builder.Where(x => x.PropertyEditorAlias == propertyEditorAlias);
                var definitions = repository.GetByQuery(query);

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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
                var collection = repository.GetPreValuesCollectionByDataTypeId(id);
                //now convert the collection to a string list
                var list = collection.FormatAsDictionary()
                    .Select(x => x.Value.Value)
                    .ToList();
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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetPreValueAsString(id);
            }
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> Move(IDataTypeDefinition toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (Moving.IsRaisedEventCancelled(
                  new MoveEventArgs<IDataTypeDefinition>(evtMsgs, new MoveEventInfo<IDataTypeDefinition>(toMove, toMove.Path, parentId)),
                  this))
            {
                return Attempt.Fail(
                    new OperationStatus<MoveOperationStatusType>(
                        MoveOperationStatusType.FailedCancelledByEvent, evtMsgs));
            }

            var moveInfo = new List<MoveEventInfo<IDataTypeDefinition>>();
            var uow = UowProvider.GetUnitOfWork();
            using (var containerRepository = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DataTypeContainerGuid))
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
            {
                try
                {
                    EntityContainer container = null;
                    if (parentId > 0)
                    {
                        container = containerRepository.Get(parentId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                    }
                    moveInfo.AddRange(repository.Move(toMove, container));
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    return Attempt.Fail(
                        new OperationStatus<MoveOperationStatusType>(ex.Operation, evtMsgs));
                }
                uow.Commit();
            }

            Moved.RaiseEvent(new MoveEventArgs<IDataTypeDefinition>(false, evtMsgs, moveInfo.ToArray()), this);

            return Attempt.Succeed(
                new OperationStatus<MoveOperationStatusType>(MoveOperationStatusType.Success, evtMsgs));
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

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
            {
                dataTypeDefinition.CreatorId = userId;
                repository.AddOrUpdate(dataTypeDefinition);
                uow.Commit();

                Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
            }

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

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
            {
                foreach (var dataTypeDefinition in dataTypeDefinitions)
                {
                    dataTypeDefinition.CreatorId = userId;
                    repository.AddOrUpdate(dataTypeDefinition);
                }
                uow.Commit();

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

            using (var uow = UowProvider.GetUnitOfWork())
            {
                using (var transaction = uow.Database.GetTransaction())
                {
                    var sortOrderObj =
                    uow.Database.ExecuteScalar<object>(
                        "SELECT max(sortorder) FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId", new { DataTypeId = dataTypeId });
                    int sortOrder;
                    if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out sortOrder) == false)
                    {
                        sortOrder = 1;
                    }

                    foreach (var value in values)
                    {
                        var dto = new DataTypePreValueDto { DataTypeNodeId = dataTypeId, Value = value, SortOrder = sortOrder };
                        uow.Database.Insert(dto);
                        sortOrder++;
                    }

                    transaction.Complete();
                }
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

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
            {
                repository.AddOrUpdatePreValues(dataTypeDefinition, values);
                uow.Commit();
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

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
            {
                dataTypeDefinition.CreatorId = userId;

                //add/update the dtd
                repository.AddOrUpdate(dataTypeDefinition);

                //add/update the prevalues
                repository.AddOrUpdatePreValues(dataTypeDefinition, values);

                uow.Commit();

                Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
            }
            
            Audit(AuditType.Save, string.Format("Save DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
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
	        
			var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateDataTypeDefinitionRepository(uow))
	        {
                repository.Delete(dataTypeDefinition);

		        uow.Commit();

		        Deleted.RaiseEvent(new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this); 		        
	        }

	        Audit(AuditType.Delete, string.Format("Delete DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
        }

        /// <summary>
        /// Gets the <see cref="IDataType"/> specified by it's unique ID
        /// </summary>
        /// <param name="id">Id of the DataType, which corresponds to the Guid Id of the control</param>
        /// <returns><see cref="IDataType"/> object</returns>
        [Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
        public IDataType GetDataTypeById(Guid id)
        {
            return DataTypesResolver.Current.GetById(id);
        }

        /// <summary>
        /// Gets a complete list of all registered <see cref="IDataType"/>'s
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        [Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
        public IEnumerable<IDataType> GetAllDataTypes()
        {
            return DataTypesResolver.Current.DataTypes;
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var auditRepo = RepositoryFactory.CreateAuditRepository(uow))
            {
                auditRepo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Commit();
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