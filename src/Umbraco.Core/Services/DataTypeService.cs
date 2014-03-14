using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
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
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/> and <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : IDataTypeService
    {
	    private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public DataTypeService()
            : this(new RepositoryFactory())
        {}

        public DataTypeService(RepositoryFactory repositoryFactory)
			: this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public DataTypeService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

		public DataTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
			_repositoryFactory = repositoryFactory;
            _uowProvider = provider;
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(_uowProvider.GetUnitOfWork()))
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
            using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(_uowProvider.GetUnitOfWork()))
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
            using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(_uowProvider.GetUnitOfWork()))
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
            using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(_uowProvider.GetUnitOfWork()))
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
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var dtos = uow.Database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new {Id = id});
                var list = dtos.Select(x => x.Value).ToList();
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
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var dtos = uow.Database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = id });
                var list = dtos.Select(x => new Tuple<PreValue, string, int>(new PreValue(x.Id, x.Value), x.Alias, x.SortOrder)).ToList();

                return PreValueConverter.ConvertToPreValuesCollection(list);
            }
        }

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var dto = uow.Database.FirstOrDefault<DataTypePreValueDto>("WHERE id = @Id", new { Id = id });
                return dto != null ? dto.Value : string.Empty;
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

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(uow))
                {
                    dataTypeDefinition.CreatorId = userId;
                    repository.AddOrUpdate(dataTypeDefinition);
                    uow.Commit();

                    Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
                }
            }

            Audit.Add(AuditTypes.Save, string.Format("Save DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, int userId = 0)
        {
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitions), this))
                return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(uow))
                {
                    foreach (var dataTypeDefinition in dataTypeDefinitions)
                    {
                        dataTypeDefinition.CreatorId = userId;
                        repository.AddOrUpdate(dataTypeDefinition);
                    }
                    uow.Commit();

                    Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinitions, false), this);
                }
            }
            Audit.Add(AuditTypes.Save, string.Format("Save DataTypeDefinition performed by user"), userId, -1);
        }

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="id">Id of the DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of string values to save</param>
        [Obsolete("This should no longer be used, use the alternative SavePreValues or SaveDataTypeAndPreValues methods instead. This will only insert pre-values without keys")]
        public void SavePreValues(int id, IEnumerable<string> values)
        {
            //TODO: Should we raise an event here since we are really saving values for the data type?

            using (new WriteLock(Locker))
            {
                using (var uow = _uowProvider.GetUnitOfWork())
                {
                    using (var transaction = uow.Database.GetTransaction())
                    {
                        var sortOrderObj =
                        uow.Database.ExecuteScalar<object>(
                            "SELECT max(sortorder) FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId", new { DataTypeId = id });
                        int sortOrder;
                        if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out sortOrder) == false)
                        {
                            sortOrder = 1;
                        }

                        foreach (var value in values)
                        {
                            var dto = new DataTypePreValueDto { DataTypeNodeId = id, Value = value, SortOrder = sortOrder };
                            uow.Database.Insert(dto);
                            sortOrder++;
                        }

                        transaction.Complete();
                    }
                }
            }
        }

        /// <summary>
        /// Saves/updates the pre-values
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <remarks>
        /// We need to actually look up each pre-value and maintain it's id if possible - this is because of silly property editors
        /// like 'dropdown list publishing keys'
        /// </remarks>
        public void SavePreValues(int id, IDictionary<string, PreValue> values)
        {
            //TODO: Should we raise an event here since we are really saving values for the data type?

            using (new WriteLock(Locker))
            {
                using (var uow = _uowProvider.GetUnitOfWork())
                {
                    using (var transaction = uow.Database.GetTransaction())
                    {
                        AddOrUpdatePreValues(id, values, uow);
                        transaction.Complete();
                    }
                }
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

            using (new WriteLock(Locker))
            {
                var uow = (PetaPocoUnitOfWork)_uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(uow))
                {
                    dataTypeDefinition.CreatorId = userId;
                    repository.AddOrUpdate(dataTypeDefinition);

                    //complete the transaction, but run the delegate before the db transaction is finalized
                    uow.Commit(database => AddOrUpdatePreValues(dataTypeDefinition.Id, values, uow));

                    Saved.RaiseEvent(new SaveEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this);
                }
            }
            
            Audit.Add(AuditTypes.Save, string.Format("Save DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
        }

        private void AddOrUpdatePreValues(int id, IDictionary<string, PreValue> preValueCollection, IDatabaseUnitOfWork uow)
        {
            //first just get all pre-values for this data type so we can compare them to see if we need to insert or update or replace
            var sql = new Sql().Select("*")
                               .From<DataTypePreValueDto>()
                               .Where<DataTypePreValueDto>(dto => dto.DataTypeNodeId == id)
                               .OrderBy<DataTypePreValueDto>(dto => dto.SortOrder);
            var currentVals = uow.Database.Fetch<DataTypePreValueDto>(sql).ToArray();

            //already existing, need to be updated
            var valueIds = preValueCollection.Where(x => x.Value.Id > 0).Select(x => x.Value.Id).ToArray();
            var existingByIds = currentVals.Where(x => valueIds.Contains(x.Id)).ToArray();

            //These ones need to be removed from the db, they no longer exist in the new values
            var deleteById = currentVals.Where(x => valueIds.Contains(x.Id) == false);

            foreach (var d in deleteById)
            {
                uow.Database.Execute(
                    "DELETE FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId AND id=@Id",
                    new { DataTypeId = id, Id = d.Id });
            }

            var sortOrder = 1;

            foreach (var pre in preValueCollection)
            {
                var existing = existingByIds.FirstOrDefault(valueDto => valueDto.Id == pre.Value.Id);
                if (existing != null)
                {
                    existing.Value = pre.Value.Value;
                    existing.SortOrder = sortOrder;
                    uow.Database.Update(existing);
                }
                else
                {
                    var dto = new DataTypePreValueDto
                    {
                        DataTypeNodeId = id,
                        Value = pre.Value.Value,
                        SortOrder = sortOrder,
                        Alias = pre.Key
                    };
                    uow.Database.Insert(dto);
                }

                sortOrder++;
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
	        if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition), this)) 
				return;
	        
			var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
	        {
		        //Find ContentTypes using this IDataTypeDefinition on a PropertyType
		        var query = Query<PropertyType>.Builder.Where(x => x.DataTypeDefinitionId == dataTypeDefinition.Id);
		        var contentTypes = repository.GetByQuery(query);

		        //Loop through the list of results and remove the PropertyTypes that references the DataTypeDefinition that is being deleted
		        foreach (var contentType in contentTypes)
		        {
			        if (contentType == null) continue;

			        foreach (var group in contentType.PropertyGroups)
			        {
				        var types = @group.PropertyTypes.Where(x => x.DataTypeDefinitionId == dataTypeDefinition.Id).ToList();
				        foreach (var propertyType in types)
				        {
					        @group.PropertyTypes.Remove(propertyType);
				        }
			        }

			        repository.AddOrUpdate(contentType);
		        }

		        var dataTypeRepository = _repositoryFactory.CreateDataTypeDefinitionRepository(uow);
		        dataTypeRepository.Delete(dataTypeDefinition);

		        uow.Commit();

		        Deleted.RaiseEvent(new DeleteEventArgs<IDataTypeDefinition>(dataTypeDefinition, false), this); 		        
	        }

	        Audit.Add(AuditTypes.Delete, string.Format("Delete DataTypeDefinition performed by user"), userId, dataTypeDefinition.Id);
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

        internal static class PreValueConverter
        {
            /// <summary>
            /// Converts the tuple to a pre-value collection
            /// </summary>
            /// <param name="list"></param>
            /// <returns></returns>
            internal static PreValueCollection ConvertToPreValuesCollection(IEnumerable<Tuple<PreValue, string, int>> list)
            {
                //now we need to determine if they are dictionary based, otherwise they have to be array based
                var dictionary = new Dictionary<string, PreValue>();

                //need to check all of the keys, if there's only one and it is empty then it's an array
                var keys = list.Select(x => x.Item2).Distinct().ToArray();
                if (keys.Length == 1 && keys[0].IsNullOrWhiteSpace())
                {
                    return new PreValueCollection(list.OrderBy(x => x.Item3).Select(x => x.Item1));
                }

                foreach (var item in list
                    .OrderBy(x => x.Item3) //we'll order them first so we maintain the order index in the dictionary
                    .GroupBy(x => x.Item2)) //group by alias
                {
                    if (item.Count() > 1)
                    {
                        //if there's more than 1 item per key, then it cannot be a dictionary, just return the array
                        return new PreValueCollection(list.OrderBy(x => x.Item3).Select(x => x.Item1));
                    }

                    dictionary.Add(item.Key, item.First().Item1);
                }

                return new PreValueCollection(dictionary);
            }
        }
    }
}