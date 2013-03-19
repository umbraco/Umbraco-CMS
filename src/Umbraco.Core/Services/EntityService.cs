using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class EntityService : IService
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly RepositoryFactory _repositoryFactory;

        public EntityService()
            : this(new RepositoryFactory())
        { }

        public EntityService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        { }

        public EntityService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        { }

        public EntityService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            _uowProvider = provider;
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Gets an UmbracoEntity by its Id, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="id">Id of the object to retrieve</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c>.</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public virtual IUmbracoEntity Get(int id, bool loadBaseType = true)
        {
            if (loadBaseType)
            {
                using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
                {
                    return repository.Get(id);
                }
            }

            //TODO Implementing loading from the various services
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an UmbracoEntity by its Id and specified Type. Optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <typeparam name="T">Type of the model to retrieve. Must be based on an <see cref="IUmbracoEntity"/></typeparam>
        /// <param name="id">Id of the object to retrieve</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c>.</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public virtual IUmbracoEntity Get<T>(int id, bool loadBaseType = true) where T : IUmbracoEntity
        {
            if (loadBaseType)
            {
                using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
                {
                    return repository.Get(id);
                }
            }

            //TODO Implementing loading from the various services
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IUmbracoEntity GetParent(int id)
        {
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                if (entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
                    return null;

                return repository.Get(entity.ParentId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.ParentId == id);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetDescendents(int id)
        {
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.Path.StartsWith(entity.Path) && x.Id != id);
                var entities = repository.GetByQuery(query);

                return entities;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetRootEntities(UmbracoObjectTypes objectType)
        {
            var objectTypeId = objectType.GetGuid();
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.ParentId == -1);
                var entities = repository.GetByQuery(query, objectTypeId);

                return entities;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll<T>() where T : IUmbracoEntity
        {
            //TODO Implement this so the type passed in is verified against types on UmbracoObjectTypes
            //and then used to get all through the method below.
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes objectType)
        {
            var objectTypeId = objectType.GetGuid();
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(objectTypeId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectTypeId"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId)
        {
            using (var repository = _repositoryFactory.CreateEntityRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(objectTypeId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual UmbracoObjectTypes GetObjectType(int id)
        {
            //TODO Implement so the entity is fetched from the db and then the Guid is used to resolve the UmbracoObjectType
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual UmbracoObjectTypes GetObjectType(IUmbracoEntity entity)
        {
            //TODO Implement this so the entity is cast to UmbracoEntity - if valid get the guid id and then resolve the UmbracoObjectType
            //otherwise fetch the IUmbracoEntity from the db and do it similar to above.
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Type GetModelType(int id)
        {
            //TODO Implement so the IUmbracoEntity is fetched from the db and then used to resolve the real type, ie. IContent, IMedia etc.
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public virtual Type GetModelType(UmbracoObjectTypes objectType)
        {
            //TODO Implement so the real type is returned fro the UmbracoObjectType's attribute.
            throw new NotImplementedException();
        }
    }
}