using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
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
            throw new NotImplementedException();
        }

        public virtual IUmbracoEntity GetParent(int id)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IUmbracoEntity> GetChildren(int id)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IUmbracoEntity> GetDescendents(int id)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll<T>() where T : IUmbracoEntity
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes objectType)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId)
        {
            throw new NotImplementedException();
        }

        public virtual UmbracoObjectTypes GetObjectType(int id)
        {
            throw new NotImplementedException();
        }

        public virtual UmbracoObjectTypes GetObjectType(IUmbracoEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual Type GetModelType(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Type GetModelType(UmbracoObjectTypes objectType)
        {
            throw new NotImplementedException();
        }
    }
}