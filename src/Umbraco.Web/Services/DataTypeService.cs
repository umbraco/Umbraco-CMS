using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using umbraco.interfaces;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/> and <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : IDataTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DataTypeService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public DataTypeService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(_unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="id">Id of the DataType control</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(Guid id)
        {
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(_unitOfWork);

            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.ControlId == id);
            var definitions = repository.GetByQuery(query);

            return definitions.FirstOrDefault();
        }

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        public IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids)
        {
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(_unitOfWork);
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataTypeDefinition dataTypeDefinition, int userId)
        {
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(_unitOfWork);
            repository.AddOrUpdate(dataTypeDefinition);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataTypeDefinition"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataTypeDefinition"/>.
        /// </remarks>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to delete</param>
        /// <param name="userId">Id of the user issueing the deletion</param>
        public void Delete(IDataTypeDefinition dataTypeDefinition, int userId)
        {           
            //Find ContentTypes using this IDataTypeDefinition on a PropertyType
            var contentTypeRepository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);
            var query = Query<PropertyType>.Builder.Where(x => x.DataTypeId == dataTypeDefinition.Id);
            var contentTypes = contentTypeRepository.GetByQuery(query);

            //Loop through the list of results and remove the PropertyTypes that references the DataTypeDefinition that is being deleted
            foreach (var contentType in contentTypes)
            {
                if(contentType == null) continue;

                foreach (var group in contentType.PropertyGroups)
                {
                    var types = group.PropertyTypes.Where(x => x.DataTypeId == dataTypeDefinition.Id);
                    foreach (var propertyType in types)
                    {
                        group.PropertyTypes.Remove(propertyType);
                    }
                }

                contentTypeRepository.AddOrUpdate(contentType);
            }

            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(_unitOfWork);
            repository.Delete(dataTypeDefinition);
            _unitOfWork.Commit();
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
    }
}