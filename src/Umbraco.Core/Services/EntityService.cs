using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services
{
    public class EntityService : ScopeRepositoryService, IEntityService
    {
        private readonly IEntityRepository _entityRepository;
        private readonly Dictionary<string, Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>> _supportedObjectTypes;
        private IQuery<IUmbracoEntity> _queryRootEntity;
        private readonly IdkMap _idkMap;

        public EntityService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
           IContentService contentService, IContentTypeService contentTypeService,
           IMediaService mediaService, IMediaTypeService mediaTypeService,
           IDataTypeService dataTypeService,
           IMemberService memberService, IMemberTypeService memberTypeService, IdkMap idkMap,
           IRuntimeCacheProvider runtimeCache,
           IEntityRepository entityRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _idkMap = idkMap;
            _entityRepository = entityRepository;

            _supportedObjectTypes = new Dictionary<string, Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>>
            {
                {typeof (IDataTypeDefinition).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.DataType, dataTypeService.GetDataTypeDefinitionById)},
                {typeof (IContent).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Document, contentService.GetById)},
                {typeof (IContentType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.DocumentType, contentTypeService.Get)},
                {typeof (IMedia).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Media, mediaService.GetById)},
                {typeof (IMediaType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.MediaType, mediaTypeService.Get)},
                {typeof (IMember).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.Member, memberService.GetById)},
                {typeof (IMemberType).FullName, new Tuple<UmbracoObjectTypes, Func<int, IUmbracoEntity>>(UmbracoObjectTypes.MemberType, memberTypeService.Get)},
            };
        }

        #region Static Queries

        // lazy-constructed because when the ctor runs, the query factory may not be ready

        private IQuery<IUmbracoEntity> QueryRootEntity => _queryRootEntity
            ?? (_queryRootEntity = Query<IUmbracoEntity>().Where(x => x.ParentId == -1));

        #endregion

        /// <summary>
        /// Returns the integer id for a given GUID
        /// </summary>
        /// <param name="key"></param>
        /// <param name="umbracoObjectType"></param>
        /// <returns></returns>
        public Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            return _idkMap.GetIdForKey(key, umbracoObjectType);
        }

        public Attempt<int> GetIdForUdi(Udi udi)
        {
            return _idkMap.GetIdForUdi(udi);
        }

        /// <summary>
        /// Returns the GUID for a given integer id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="umbracoObjectType"></param>
        /// <returns></returns>
        public Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType)
        {
            return _idkMap.GetKeyForId(id, umbracoObjectType);
        }

        public IUmbracoEntity GetByKey(Guid key, bool loadBaseType = true)
        {
            if (loadBaseType)
            {
                using (var scope = ScopeProvider.CreateScope(readOnly: true))
                {                    
                    return _entityRepository.GetByKey(key);
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
                using (var scope = ScopeProvider.CreateScope(readOnly: true))
                {
                    return _entityRepository.Get(id);
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
                using (var scope = ScopeProvider.CreateScope(readOnly: true))
                {
                    return _entityRepository.GetByKey(key, objectTypeId);
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
                using (var scope = ScopeProvider.CreateScope(readOnly: true))
                {
                    return _entityRepository.Get(id, objectTypeId);
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
                using (var scope = ScopeProvider.CreateScope(readOnly: true))
                {
                    return _entityRepository.Get(id);
                }
            }

            var typeFullName = typeof(T).FullName;
            if (_supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported");
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var entity = _entityRepository.Get(id);
                if (entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
                    return null;

                return _entityRepository.Get(entity.ParentId);
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var entity = _entityRepository.Get(id);
                if (entity.ParentId == -1 || entity.ParentId == -20 || entity.ParentId == -21)
                    return null;

                var objectTypeId = umbracoObjectType.GetGuid();
                return _entityRepository.Get(entity.ParentId, objectTypeId);
            }
        }

        /// <summary>
        /// Gets a collection of children by the parents Id
        /// </summary>
        /// <param name="parentId">Id of the parent to retrieve children for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetChildren(int parentId)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId);
                return _entityRepository.GetByQuery(query);
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId);
                return _entityRepository.GetByQuery(query, objectTypeId).ToList(); // run within using! // run within using!
            }
        }

        /// <summary>
        /// Gets a collection of descendents by the parents Id
        /// </summary>
        /// <param name="id">Id of entity to retrieve descendents for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetDescendents(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var entity = _entityRepository.Get(id);
                var pathMatch = entity.Path + ",";
                var query = Query<IUmbracoEntity>().Where(x => x.Path.StartsWith(pathMatch) && x.Id != id);
                return _entityRepository.GetByQuery(query);
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var entity = _entityRepository.Get(id);
                var query = Query<IUmbracoEntity>().Where(x => x.Path.StartsWith(entity.Path) && x.Id != id);
                return _entityRepository.GetByQuery(query, objectTypeId);
            }
        }

        /// <summary>
        /// Returns a paged collection of children
        /// </summary>
        /// <param name="parentId">The parent id to return children for</param>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IUmbracoEntity> GetPagedChildren(int parentId, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "SortOrder", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>().Where(x => x.ParentId == parentId && x.Trashed == false);

                IQuery<IUmbracoEntity> filterQuery = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter));
                }

                var contents = _entityRepository.GetPagedResultsByQuery(query, objectTypeId, pageIndex, pageSize, out totalRecords, orderBy, orderDirection, filterQuery);
                return contents;
            }
        }

        /// <summary>
        /// Returns a paged collection of descendants
        /// </summary>
        /// <param name="id"></param>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IUmbracoEntity> GetPagedDescendants(int id, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>();
                //if the id is System Root, then just get all
                //if the id is System Root, then just get all

                if (id != Constants.System.Root)
                {
                    //lookup the path so we can use it in the prefix query below
                    var itemPaths = _entityRepository.GetAllPaths(objectTypeId, id).ToArray();
                    if (itemPaths.Length == 0)
                    {
                        totalRecords = 0;
                        return Enumerable.Empty<IUmbracoEntity>();
                    }
                    var itemPath = itemPaths[0].Path;

                    query.Where(x => x.Path.SqlStartsWith(itemPath + ",", TextColumnType.NVarchar));
                }
                IQuery<IUmbracoEntity> filterQuery = null;

                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter));
                }
                var contents = _entityRepository.GetPagedResultsByQuery(query, objectTypeId, pageIndex, pageSize, out totalRecords, orderBy, orderDirection, filterQuery);
                return contents;
            }
        }
        /// <summary>
        /// Returns a paged collection of descendants.
        /// </summary>
        public IEnumerable<IUmbracoEntity> GetPagedDescendants(IEnumerable<int> ids, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            totalRecords = 0;

            var idsA = ids.ToArray();
            if (idsA.Length == 0)
                return Enumerable.Empty<IUmbracoEntity>();

            var objectTypeId = umbracoObjectType.GetGuid();

            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>();
                if (idsA.All(x => x != Constants.System.Root))
                {
                    //lookup the paths so we can use it in the prefix query below
                    //lookup the paths so we can use it in the prefix query below
                    var itemPaths = _entityRepository.GetAllPaths(objectTypeId, idsA).ToArray();

                    if (itemPaths.Length == 0)
                    {
                        totalRecords = 0;
                        return Enumerable.Empty<IUmbracoEntity>();
                    }
                    var clauses = new List<Expression<Func<IUmbracoEntity, bool>>>();
                    foreach (var id in idsA)
                    {
                        //if the id is root then don't add any clauses
                        if (id != Constants.System.Root)
                        {
                            var itemPath = itemPaths.FirstOrDefault(x => x.Id == id);
                            if (itemPath == null) continue;
                            var path = itemPath.Path;
                            var qid = id;
                            clauses.Add(x => x.Path.SqlStartsWith(path + ",", TextColumnType.NVarchar) || x.Path.SqlEndsWith("," + qid, TextColumnType.NVarchar));
                        }
                    }
                    query.WhereAny(clauses);
                }

                IQuery<IUmbracoEntity> filterQuery = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter));
                }

                var contents = _entityRepository.GetPagedResultsByQuery(query, objectTypeId, pageIndex, pageSize, out totalRecords, orderBy, orderDirection, filterQuery);
                return contents;
            }
        }

        /// <summary>
        /// Returns a paged collection of descendants from the root
        /// </summary>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <param name="includeTrashed">true/false to include trashed objects</param>
        /// <returns></returns>
        public IEnumerable<IUmbracoEntity> GetPagedDescendantsFromRoot(UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "", bool includeTrashed = true)
        {
            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var query = Query<IUmbracoEntity>();
                //don't include trashed if specfied
                //don't include trashed if specfied

                if (includeTrashed == false)
                {
                    query.Where(x => x.Trashed == false);
                }
                IQuery<IUmbracoEntity> filterQuery = null;

                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter));
                }
                var contents = _entityRepository.GetPagedResultsByQuery(query, objectTypeId, pageIndex, pageSize, out totalRecords, orderBy, orderDirection, filterQuery);
                return contents;
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetByQuery(QueryRootEntity, objectTypeId);
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
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported");

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
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported");

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetAll(objectTypeId, ids);
            }
        }

        public IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes umbracoObjectType, Guid[] keys)
        {
            var entityType = GetEntityType(umbracoObjectType);

            var typeFullName = entityType.FullName;
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported");

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetAll(objectTypeId, keys);
            }
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(UmbracoObjectTypes umbracoObjectType, params int[] ids)
        {
            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported.");

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetAllPaths(objectTypeId, ids);
            }
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(UmbracoObjectTypes umbracoObjectType, params Guid[] keys)
        {
            var entityType = GetEntityType(umbracoObjectType);
            var typeFullName = entityType.FullName;
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported.");

            var objectTypeId = umbracoObjectType.GetGuid();
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetAllPaths(objectTypeId, keys);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="T:Umbraco.Core.Models.EntityBase.IUmbracoEntity" />
        /// </summary>
        /// <param name="objectTypeId">Guid id of the UmbracoObjectType</param>
        /// <param name="ids"></param>
        /// <returns>An enumerable list of <see cref="T:Umbraco.Core.Models.EntityBase.IUmbracoEntity" /> objects</returns>
        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids)
        {
            var umbracoObjectType = UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
            var entityType = GetEntityType(umbracoObjectType);

            var typeFullName = entityType.FullName;
            if (typeFullName == null || _supportedObjectTypes.ContainsKey(typeFullName) == false)
                throw new NotSupportedException("The passed in type is not supported");

            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                return _entityRepository.GetAll(objectTypeId, ids);
            }
        }

        /// <summary>
        /// Gets the UmbracoObjectType from the integer id of an IUmbracoEntity.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        public virtual UmbracoObjectTypes GetObjectType(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("nodeObjectType")
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeId == id);
                var nodeObjectTypeId = scope.Database.ExecuteScalar<Guid>(sql);
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
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("nodeObjectType")
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.UniqueId == key);
                var nodeObjectTypeId = scope.Database.ExecuteScalar<Guid>(sql);
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
            return entity is UmbracoEntity entityImpl
                ? UmbracoObjectTypesExtensions.GetUmbracoObjectType(entityImpl.NodeObjectTypeId)
                : GetObjectType(entity.Id);
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

        public bool Exists(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {                
                var exists = _entityRepository.Exists(key);
                return exists;
            }
        }

        public bool Exists(int id)
        {
            using (var scope = ScopeProvider.CreateScope(readOnly: true))
            {                
                var exists = _entityRepository.Exists(id);
                return exists;
            }
        }

        /// <inheritdoc />
        public int ReserveId(Guid key)
        {
            NodeDto node;
            using (var scope = ScopeProvider.CreateScope())
            {
                var sql = new Sql("SELECT * FROM umbracoNode WHERE uniqueID=@0 AND nodeObjectType=@1", key, Constants.ObjectTypes.IdReservation);
                node = scope.Database.SingleOrDefault<NodeDto>(sql);
                if (node != null) throw new InvalidOperationException("An identifier has already been reserved for this Udi.");
                node = new NodeDto
                {
                    UniqueId = key,
                    Text = "RESERVED.ID",
                    NodeObjectType = Constants.ObjectTypes.IdReservation,

                    CreateDate = DateTime.Now,
                    UserId = 0,
                    ParentId = -1,
                    Level = 1,
                    Path = "-1",
                    SortOrder = 0,
                    Trashed = false
                };
                scope.Database.Insert(node);
                scope.Complete();
            }
            return node.NodeId;
        }
    }
}
