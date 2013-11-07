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
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByControlId(Guid id)
        {
            using (var repository = _repositoryFactory.CreateDataTypeDefinitionRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IDataTypeDefinition>.Builder.Where(x => x.ControlId == id);
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
        /// Gets all prevalues for an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// This method should be kept internal until a proper PreValue object model is introduced.
        /// </remarks>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of Tuples containing Id, Alias, SortOrder, Value</returns>
        internal IEnumerable<Tuple<int, string, int, string>> GetDetailedPreValuesByDataTypeId(int id)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var dtos = uow.Database.Fetch<DataTypePreValueDto>("WHERE datatypeNodeId = @Id", new { Id = id });
                var list = dtos.Select(x => new Tuple<int, string, int, string>(x.Id, x.Alias, x.SortOrder, x.Value)).ToList();
                return list;
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
        public void SavePreValues(int id, IEnumerable<string> values)
        {
            SavePreValues(id, values.Select(x => new Tuple<string, string>(string.Empty, x)));
        }

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="id">Id of the DataTypeDefinition to save PreValues for</param>
        /// <param name="prevalues">List of prevalues to save</param>
        public void SavePreValues(int id, IEnumerable<Tuple<string, string>> prevalues)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = _uowProvider.GetUnitOfWork())
                {
                    var sortOrderObj =
                        uow.Database.ExecuteScalar<object>(
                            "SELECT max(sortorder) FROM cmsDataTypePreValues WHERE datatypeNodeId = @DataTypeId", new { DataTypeId = id });
                    int sortOrder;
                    if (sortOrderObj == null || int.TryParse(sortOrderObj.ToString(), out sortOrder) == false)
                    {
                        sortOrder = 1;
                    }

                    using (var transaction = uow.Database.GetTransaction())
                    {
                        foreach (var prevalue in prevalues)
                        {
                            var dto = new DataTypePreValueDto { DataTypeNodeId = id, Value = prevalue.Item2, SortOrder = sortOrder, Alias = prevalue.Item1 };
                            uow.Database.Insert(dto);
                            sortOrder++;
                        }

                        transaction.Complete();
                    }
                }
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
        public IDataType GetDataTypeById(Guid id)
        {
            return DataTypesResolver.Current.GetById(id);
        }

        /// <summary>
        /// Gets a complete list of all registered <see cref="IDataType"/>'s
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
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
    }
}