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

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : RepositoryService, IDataTypeService
    {

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public DataTypeService()
            : this(new RepositoryFactory())
        {}

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public DataTypeService(RepositoryFactory repositoryFactory)
			: this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public DataTypeService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
		public DataTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger)
        {
        }

        public DataTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

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
        #endregion

        
    }
}