using System;
using System.Collections.Generic;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class EntityService : RepositoryService, IEntityService
    {
        private readonly Dictionary<string, Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>> _supportedObjectTypes;

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public EntityService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, 
            IContentService contentService, IContentTypeService contentTypeService, IMediaService mediaService, IDataTypeService dataTypeService,
            IMemberService memberService, IMemberTypeService memberTypeService)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger, contentService, contentTypeService, mediaService,
            dataTypeService, memberService, memberTypeService)
        {
            
        }

        public EntityService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger,
           IContentService contentService, IContentTypeService contentTypeService, IMediaService mediaService, IDataTypeService dataTypeService,
           IMemberService memberService, IMemberTypeService memberTypeService)
            : base(provider, repositoryFactory, logger)
        {
            IContentTypeService contentTypeService1 = contentTypeService;

            _supportedObjectTypes = new Dictionary<string, Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>>
            {
                {typeof (IDataTypeDefinition).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.DataType, dataTypeService.GetDataTypeDefinitionById)},
                {typeof (IContent).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Document, contentService.GetById)},
                {typeof (IContentType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.DocumentType, contentTypeService1.GetContentType)},
                {typeof (IMedia).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Media, mediaService.GetById)},
                {typeof (IMediaType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.MediaType, contentTypeService1.GetMediaType)},
                {typeof (IMember).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Member, memberService.GetById)},
                {typeof (IMemberType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.MemberType, memberTypeService.Get)}
            };
        }

        public IUmbracoEntity GetByKey(Guid key, bool loadBaseType = true)
        {
            if (loadBaseType)
            {
                using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
                {
                    return repository.GetByKey(key);
                }
            }

            //SD: TODO: Need to enable this at some stage ... just need to ask Morten what the deal is with what this does.
            throw new NotSupportedException();

            //var objectType = GetObjectType(key);
            //var entityType = GetEntityType(objectType);
            //var typeFullName = entityType.FullName;
            //var entity = _supportedObjectTypes[typeFullName].Item2(id);

            //return entity;
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
                using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
                {
                    return repository.Get(id);
                }
            }

            var objectType = GetObjectType(id);
            var entityType = GetEntityType(objectType);
            var typeFullName = entityType.FullName;
            var entity = _supportedObjectTypes[typeFullName].Item2(id);

            return entity;
        }

        public IUmbracoEntity GetByKey(Guid key, UmbracoObjectTypes umbracoObjectType, bool loadBaseType = true)
        {
            if (loadBaseType)
            {
                var objectTypeId = umbracoObjectType.GetGuid();
                using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
                {
                    return repository.GetByKey(key, objectTypeId);
                }
            }

            //SD: TODO: Need to enable this at some stage ... just need to ask Morten what the deal is with what this does.
            throw new NotSupportedException();

            //var entityType = GetEntityType(umbracoObjectType);
            //var typeFullName = entityType.FullName;
            //var entity = _supportedObjectTypes[typeFullName].Item2(id);

            //return entity;
        }

        /// <summary>
        /// Gets an UmbracoEntity by its Id and UmbracoObjectType, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="id">Id of the object to retrieve</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entity to retrieve</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c>.</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public virtual IUmbracoEntity Get(int id, UmbracoObjectTypes umbracoObjectType, bool loadBaseType = true)
        {
            if (loadBaseType)
            {
                var objectTypeId = umbracoObjectType.GetGuid();
                using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
                {
                    return repository.Get(id, objectTypeId);
                }
            }

            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            var entity = _supportedObjectTypes[typeFullName].Item2(id);

            return entity;
        }

        public IUmbracoEntity GetByKey<T>(Guid key, bool loadBaseType = true) where T : IUmbracoEntity
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
            if (loadBaseType)
            {
                using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
                {
                    return repository.Get(id);
                }
            }

            var typeFullName = typeof(T).FullName;
            Mandate.That<NotSupportedException>(_supportedObjectTypes.ContainsKey(typeFullName), () =>
            {
                throw new NotSupportedException
                    ("The passed in type is not supported");
            });
            var entity = _supportedObjectTypes[typeFullName].Item2(id);

            return entity;
        }

        /// <summary>
        /// Gets the parent of entity by its id
        /// </summary>
        /// <param name="id">Id of the entity to retrieve the Parent for</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public virtual IUmbracoEntity GetParent(int id)
        {
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                if (entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
                    return null;

                return repository.Get(entity.ParentId);
            }
        }

        /// <summary>
        /// Gets the parent of entity by its id and UmbracoObjectType
        /// </summary>
        /// <param name="id">Id of the entity to retrieve the Parent for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the parent to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public virtual IUmbracoEntity GetParent(int id, UmbracoObjectTypes umbracoObjectType)
        {
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                if (entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
                    return null;

                var objectTypeId = umbracoObjectType.GetGuid();
                return repository.Get(entity.ParentId, objectTypeId);
            }
        }

        /// <summary>
        /// Gets a collection of children by the parents Id
        /// </summary>
        /// <param name="parentId">Id of the parent to retrieve children for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetChildren(int parentId)
        {
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.ParentId == parentId);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of children by the parents Id and UmbracoObjectType
        /// </summary>
        /// <param name="parentId">Id of the parent to retrieve children for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the children to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetChildren(int parentId, UmbracoObjectTypes umbracoObjectType)
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.ParentId == parentId);
                var contents = repository.GetByQuery(query, objectTypeId);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of descendents by the parents Id
        /// </summary>
        /// <param name="id">Id of entity to retrieve descendents for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetDescendents(int id)
        {
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                var pathMatch = entity.Path + ",";
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.Path.StartsWith(pathMatch) && x.Id != id);
                var entities = repository.GetByQuery(query);

                return entities;
            }
        }

        /// <summary>
        /// Gets a collection of descendents by the parents Id
        /// </summary>
        /// <param name="id">Id of entity to retrieve descendents for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the descendents to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetDescendents(int id, UmbracoObjectTypes umbracoObjectType)
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var entity = repository.Get(id);
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.Path.StartsWith(entity.Path) && x.Id != id);
                var entities = repository.GetByQuery(query, objectTypeId);

                return entities;
            }
        }

        /// <summary>
        /// Gets a collection of the entities at the root, which corresponds to the entities with a Parent Id of -1.
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectType of the root entities to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetRootEntities(UmbracoObjectTypes umbracoObjectType)
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUmbracoEntity>.Builder.Where(x => x.ParentId == -1);
                var entities = repository.GetByQuery(query, objectTypeId);

                return entities;
            }
        }

        /// <summary>
        /// Gets a collection of all <see cref="IUmbracoEntity"/> of a given type.
        /// </summary>
        /// <typeparam name="T">Type of the entities to retrieve</typeparam>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll<T>(params int[] ids) where T : IUmbracoEntity
        {
            var typeFullName = typeof(T).FullName;
            Mandate.That<NotSupportedException>(_supportedObjectTypes.ContainsKey(typeFullName), () =>
            {
                throw new NotSupportedException
                    ("The passed in type is not supported");
            });
            var objectType = _supportedObjectTypes[typeFullName].Item1;

            return GetAll(objectType, ids);
        }

        /// <summary>
        /// Gets a collection of all <see cref="IUmbracoEntity"/> of a given type.
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entities to return</param>
        /// <param name="ids"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes umbracoObjectType, params int[] ids)
        {
            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            Mandate.That<NotSupportedException>(_supportedObjectTypes.ContainsKey(typeFullName), () =>
            {
                throw new NotSupportedException
                    ("The passed in type is not supported");
            });

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(objectTypeId, ids);
            }
        }

        public IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes umbracoObjectType, Guid[] keys)
        {
            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            Mandate.That<NotSupportedException>(_supportedObjectTypes.ContainsKey(typeFullName), () =>
            {
                throw new NotSupportedException
                    ("The passed in type is not supported");
            });

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(objectTypeId, keys);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="objectTypeId">Guid id of the UmbracoObjectType</param>
        /// <param name="ids"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids)
        {
            var umbracoObjectType = UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            Mandate.That<NotSupportedException>(_supportedObjectTypes.ContainsKey(typeFullName), () =>
            {
                throw new NotSupportedException
                    ("The passed in type is not supported");
            });

            using (var repository = RepositoryFactory.CreateEntityRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(objectTypeId, ids);
            }
        }

        /// <summary>
        /// Gets the UmbracoObjectType from the integer id of an IUmbracoEntity.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        public virtual UmbracoObjectTypes GetObjectType(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var sql = new Sql().Select("nodeObjectType").From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
                var nodeObjectTypeId = uow.Database.ExecuteScalar<Guid>(sql);
                var objectTypeId = nodeObjectTypeId;
                return UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
            }
        }

        /// <summary>
        /// Gets the UmbracoObjectType from the integer id of an IUmbracoEntity.
        /// </summary>
        /// <param name="key">Unique Id of the entity</param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        public virtual UmbracoObjectTypes GetObjectType(Guid key)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var sql = new Sql().Select("nodeObjectType").From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
                var nodeObjectTypeId = uow.Database.ExecuteScalar<Guid>(sql);
                var objectTypeId = nodeObjectTypeId;
                return UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
            }
        }

        /// <summary>
        /// Gets the UmbracoObjectType from an IUmbracoEntity.
        /// </summary>
        /// <param name="entity"><see cref="IUmbracoEntity"/></param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        public virtual UmbracoObjectTypes GetObjectType(IUmbracoEntity entity)
        {
            var entityImpl = entity as UmbracoEntity;
            if (entityImpl == null)
                return GetObjectType(entity.Id);

            return UmbracoObjectTypesExtensions.GetUmbracoObjectType(entityImpl.NodeObjectTypeId);
        }

        /// <summary>
        /// Gets the Type of an entity by its Id
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns>Type of the entity</returns>
        public virtual Type GetEntityType(int id)
        {
            var objectType = GetObjectType(id);
            return GetEntityType(objectType);
        }

        /// <summary>
        /// Gets the Type of an entity by its <see cref="UmbracoObjectTypes"/>
        /// </summary>
        /// <param name="umbracoObjectType"><see cref="UmbracoObjectTypes"/></param>
        /// <returns>Type of the entity</returns>
        public virtual Type GetEntityType(UmbracoObjectTypes umbracoObjectType)
        {
            var type = typeof(UmbracoObjectTypes);
            var memInfo = type.GetMember(umbracoObjectType.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(UmbracoObjectTypeAttribute),
                false);

            var attribute = ((UmbracoObjectTypeAttribute)attributes[0]);
            if (attribute == null)
                throw new NullReferenceException("The passed in UmbracoObjectType does not contain an UmbracoObjectTypeAttribute, which is used to retrieve the Type.");

            if (attribute.ModelType == null)
                throw new NullReferenceException("The passed in UmbracoObjectType does not contain a Type definition");

            return attribute.ModelType;
        }
    }
}