using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the EntityRepository used to query <see cref="IUmbracoEntity"/> objects.
    /// </summary>
    /// <remarks>
    /// This is limited to objects that are based in the umbracoNode-table.
    /// </remarks>
    internal class EntityRepository : DisposableObjectSlim, IEntityRepository
    {
        public EntityRepository(IDatabaseUnitOfWork work)
		{
		    UnitOfWork = work;
		}

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected internal IDatabaseUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Internal for testing purposes
        /// </summary>
        internal Guid UnitKey
        {
            get { return (Guid)UnitOfWork.Key; }
        }

        #region Query Methods

        public IEnumerable<IUmbracoEntity> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, IQuery<IUmbracoEntity> filter = null)
        {
            bool isContent = objectTypeId == Constants.ObjectTypes.DocumentGuid || objectTypeId == Constants.ObjectTypes.DocumentBlueprintGuid;
            bool isMedia = objectTypeId == Constants.ObjectTypes.MediaGuid;
            var factory = new UmbracoEntityFactory();

            var sqlClause = GetBaseWhere(GetBase, isContent, isMedia, sql =>
            {
                if (filter != null)
                {
                    foreach (var filterClause in filter.GetWhereClauses())
                    {
                        sql.Where(filterClause.Item1, filterClause.Item2);
                    }
                }
            }, objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var entitySql = translator.Translate();
            var pagedSql = entitySql.Append(GetGroupBy(isContent, isMedia, false));
            pagedSql = (orderDirection == Direction.Descending) ? pagedSql.OrderByDescending("umbracoNode.id") : pagedSql.OrderBy("umbracoNode.id");

            IEnumerable<IUmbracoEntity> result;

            var pagedResult = UnitOfWork.Database.Page<dynamic>(pageIndex + 1, pageSize, pagedSql);
            result = pagedResult.Items.Select(factory.BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();

            //The total items from the PetaPoco page query will be wrong due to the Outer join used on parent, depending on the search this will
            //return duplicate results when the COUNT is used in conjuction with it, so we need to get the total on our own.

            //generate a query that does not contain the LEFT Join for parent, this would cause
            //the COUNT(*) query to return the wrong
            var sqlCountClause = GetBaseWhere(
                (isC, isM, f) => GetBase(isC, isM, f, true), //true == is a count query
                isContent, isMedia, sql =>
                {
                    if (filter != null)
                    {
                        foreach (var filterClause in filter.GetWhereClauses())
                        {
                            sql.Where(filterClause.Item1, filterClause.Item2);
                        }
                    }
                }, objectTypeId);
            var translatorCount = new SqlTranslator<IUmbracoEntity>(sqlCountClause, query);
            var countSql = translatorCount.Translate();

            totalRecords = UnitOfWork.Database.ExecuteScalar<int>(countSql);

            return result;
        }

        public IUmbracoEntity GetByKey(Guid key)
        {
            var sql = GetBaseWhere(GetBase, false, false, key);
            var nodeDto = UnitOfWork.Database.FirstOrDefault<dynamic>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntityFromDynamic(nodeDto);

            return entity;
        }

        public IUmbracoEntity GetByKey(Guid key, Guid objectTypeId)
        {
            bool isContent = objectTypeId == Constants.ObjectTypes.DocumentGuid || objectTypeId == Constants.ObjectTypes.DocumentBlueprintGuid;
            bool isMedia = objectTypeId == Constants.ObjectTypes.MediaGuid;

            var sql = GetFullSqlForEntityType(key, isContent, isMedia, objectTypeId);

            var factory = new UmbracoEntityFactory();

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, false));
            }
            var found = collection.FirstOrDefault();
            return found != null ? found.BuildFromDynamic() : null;


        }

        public virtual IUmbracoEntity Get(int id)
        {
            var sql = GetBaseWhere(GetBase, false, false, id);
            var nodeDto = UnitOfWork.Database.FirstOrDefault<dynamic>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntityFromDynamic(nodeDto);

            return entity;
        }

        public virtual IUmbracoEntity Get(int id, Guid objectTypeId)
        {
            bool isContent = objectTypeId == Constants.ObjectTypes.DocumentGuid || objectTypeId == Constants.ObjectTypes.DocumentBlueprintGuid;
            bool isMedia = objectTypeId == Constants.ObjectTypes.MediaGuid;

            var sql = GetFullSqlForEntityType(id, isContent, isMedia, objectTypeId);

            var factory = new UmbracoEntityFactory();

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, false));
            }
            var found = collection.FirstOrDefault();
            return found != null ? found.BuildFromDynamic() : null;

        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids)
        {
            return ids.Any()
                ? PerformGetAll(objectTypeId, sql => sql.Where(" umbracoNode.id in (@ids)", new { ids }))
                : PerformGetAll(objectTypeId);
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAll(objectTypeId, sql => sql.Where(" umbracoNode.uniqueID in (@keys)", new { keys }))
                : PerformGetAll(objectTypeId);
        }

        private IEnumerable<IUmbracoEntity> PerformGetAll(Guid objectTypeId, Action<Sql> filter = null)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.DocumentGuid || objectTypeId == Constants.ObjectTypes.DocumentBlueprintGuid;
            var isMedia = objectTypeId == Constants.ObjectTypes.MediaGuid;
            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, filter);

            var factory = new UmbracoEntityFactory();

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, isMedia));
            }
            return collection.Select(x => x.BuildFromDynamic()).ToList();
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(Guid objectTypeId, params int[] ids)
        {
            return ids.Any()
                ? PerformGetAllPaths(objectTypeId, sql => sql.Append(" AND umbracoNode.id in (@ids)", new { ids }))
                : PerformGetAllPaths(objectTypeId);
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(Guid objectTypeId, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAllPaths(objectTypeId, sql => sql.Append(" AND umbracoNode.uniqueID in (@keys)", new { keys }))
                : PerformGetAllPaths(objectTypeId);
        }

        private IEnumerable<EntityPath> PerformGetAllPaths(Guid objectTypeId, Action<Sql> filter = null)
        {
            var sql = new Sql("SELECT id, path FROM umbracoNode WHERE umbracoNode.nodeObjectType=@type", new { type = objectTypeId });
            if (filter != null) filter(sql);
            return UnitOfWork.Database.Fetch<EntityPath>(sql);
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBase(false, false, null);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy(false, false));

            var dtos = UnitOfWork.Database.Fetch<dynamic>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        /// <summary>
        /// Gets entities by query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objectTypeId"></param>
        /// <returns></returns>
        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.DocumentGuid || objectTypeId == Constants.ObjectTypes.DocumentBlueprintGuid;
            var isMedia = objectTypeId == Constants.ObjectTypes.MediaGuid;

            var sqlClause = GetBaseWhere(GetBase, isContent, isMedia, null, objectTypeId);

            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var entitySql = translator.Translate();

            return GetByQueryInternal(entitySql, isContent, isMedia);
        }

        private IEnumerable<IUmbracoEntity> GetByQueryInternal(Sql entitySql, bool isContent, bool isMedia)
        {
            var factory = new UmbracoEntityFactory();

            //use dynamic so that we can get ALL properties from the SQL so we can chuck that data into our AdditionalData
            var finalSql = entitySql.Append(GetGroupBy(isContent, isMedia));

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(finalSql);
            var collection = new EntityDefinitionCollection();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, isMedia));
            }
            return collection.Select(x => x.BuildFromDynamic()).ToList();
        }

        #endregion


        #region Sql Statements

        protected Sql GetFullSqlForEntityType(Guid key, bool isContent, bool isMedia, Guid objectTypeId)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, key);

            if (isMedia == false) return entitySql.Append(GetGroupBy(isContent, false));

            return entitySql.Append(GetGroupBy(isContent, true, false));
        }

        protected Sql GetFullSqlForEntityType(int id, bool isContent, bool isMedia, Guid objectTypeId)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, id);

            if (isMedia == false) return entitySql.Append(GetGroupBy(isContent, false));

            return entitySql.Append(GetGroupBy(isContent, true, false));
        }

        protected Sql GetFullSqlForEntityType(bool isContent, bool isMedia, Guid objectTypeId, Action<Sql> filter)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, filter, objectTypeId);

            if (isMedia == false) return entitySql.Append(GetGroupBy(isContent, false));

            return entitySql.Append(GetGroupBy(isContent, true, false));
        }

        protected virtual Sql GetBase(bool isContent, bool isMedia, Action<Sql> customFilter)
        {
            return GetBase(isContent, isMedia, customFilter, false);
        }

        protected virtual Sql GetBase(bool isContent, bool isMedia, Action<Sql> customFilter, bool isCount)
        {
            var columns = new List<object>();
            if (isCount)
            {
                columns.Add("COUNT(*)");
            }
            else
            {
                columns.AddRange(new List<object>
                {
                    "umbracoNode.id",
                    "umbracoNode.trashed",
                    "umbracoNode.parentID",
                    "umbracoNode.nodeUser",
                    "umbracoNode.level",
                    "umbracoNode.path",
                    "umbracoNode.sortOrder",
                    "umbracoNode.uniqueID",
                    "umbracoNode.text",
                    "umbracoNode.nodeObjectType",
                    "umbracoNode.createDate",
                    "COUNT(parent.parentID) as children"
                });

                if (isContent || isMedia)
                {
                    if (isContent)
                    {
                        //only content has/needs this info
                        columns.Add("published.versionId as publishedVersion");
                        columns.Add("document.versionId as newestVersion");
                        columns.Add("contentversion.id as versionId");
                    }

                    columns.Add("contenttype.alias");
                    columns.Add("contenttype.icon");
                    columns.Add("contenttype.thumbnail");
                    columns.Add("contenttype.isContainer");
                }

                if (isMedia)
                {
                    columns.Add("media.mediaPath");
                }
            }

            //Creates an SQL query to return a single row for the entity

            var entitySql = new Sql()
                .Select(columns.ToArray())
                .From("umbracoNode umbracoNode");

            if (isContent || isMedia)
            {
                entitySql.InnerJoin("cmsContent content").On("content.nodeId = umbracoNode.id");

                if (isContent)
                {
                    //only content has/needs this info
                    entitySql
                        .InnerJoin("cmsDocument document").On("document.nodeId = umbracoNode.id")
                        .InnerJoin("cmsContentVersion contentversion").On("contentversion.VersionId = document.versionId")
                        .LeftJoin("(SELECT nodeId, versionId FROM cmsDocument WHERE published = 1) as published")
                        .On("umbracoNode.id = published.nodeId");
                }

                if (isMedia)
                {
                    entitySql.InnerJoin("cmsMedia media").On("media.nodeId = umbracoNode.id");
                }

                entitySql.LeftJoin("cmsContentType contenttype").On("contenttype.nodeId = content.contentType");
            }



            if (isCount == false)
            {
                entitySql.LeftJoin("umbracoNode parent").On("parent.parentID = umbracoNode.id");
            }

            if (customFilter != null)
            {
                customFilter(entitySql);
            }

            return entitySql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, Action<Sql>, Sql> baseQuery, bool isContent, bool isMedia, Action<Sql> filter, Guid nodeObjectType)
        {
            var sql = baseQuery(isContent, isMedia, filter)
                .Where("umbracoNode.nodeObjectType = @NodeObjectType", new { NodeObjectType = nodeObjectType });

            if (isContent)
            {
                sql.Where("document.newest = 1");
            }

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, Action<Sql>, Sql> baseQuery, bool isContent, bool isMedia, int id)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.id = @Id", new { Id = id });

            if (isContent)
            {
                sql.Where("document.newest = 1");
            }

            sql.Append(GetGroupBy(isContent, isMedia));

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, Action<Sql>, Sql> baseQuery, bool isContent, bool isMedia, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.uniqueID = @UniqueID", new {UniqueID = key});

            if (isContent)
            {
                sql.Where("document.newest = 1");
            }

            sql.Append(GetGroupBy(isContent, isMedia));

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, Action<Sql>, Sql> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, int id)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.id = @Id AND umbracoNode.nodeObjectType = @NodeObjectType",
                       new {Id = id, NodeObjectType = nodeObjectType});

            if (isContent)
            {
                sql.Where("document.newest = 1");
            }

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, Action<Sql>, Sql> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.uniqueID = @UniqueID AND umbracoNode.nodeObjectType = @NodeObjectType",
                       new { UniqueID = key, NodeObjectType = nodeObjectType });

            if (isContent)
            {
                sql.Where("document.newest = 1");
            }

            return sql;
        }

        protected virtual Sql GetGroupBy(bool isContent, bool isMedia, bool includeSort = true)
        {
            var columns = new List<object>
            {
                "umbracoNode.id",
                "umbracoNode.trashed",
                "umbracoNode.parentID",
                "umbracoNode.nodeUser",
                "umbracoNode.level",
                "umbracoNode.path",
                "umbracoNode.sortOrder",
                "umbracoNode.uniqueID",
                "umbracoNode.text",
                "umbracoNode.nodeObjectType",
                "umbracoNode.createDate"
            };

            if (isContent || isMedia)
            {
                if (isContent)
                {
                    columns.Add("published.versionId");
                    columns.Add("document.versionId");
                    columns.Add("contentversion.id");
                }

                if (isMedia)
                {
                    columns.Add("media.mediaPath");
                }

                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("contenttype.isContainer");
            }

            var sql = new Sql()
                .GroupBy(columns.ToArray());

            if (includeSort)
            {
                sql = sql.OrderBy("umbracoNode.sortOrder");
            }

            return sql;
        }

        #endregion

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            UnitOfWork.DisposeIfDisposable();
        }

        public bool Exists(Guid key)
        {
            var sql = new Sql().Select("COUNT(*)").From("umbracoNode").Where("uniqueID=@uniqueID", new {uniqueID = key});
            return UnitOfWork.Database.ExecuteScalar<int>(sql) > 0;
        }

        public bool Exists(int id)
        {
            var sql = new Sql().Select("COUNT(*)").From("umbracoNode").Where("id=@id", new { id = id });
            return UnitOfWork.Database.ExecuteScalar<int>(sql) > 0;
        }

        #region private classes


        private class EntityDefinitionCollection : KeyedCollection<int, EntityDefinition>
        {
            protected override int GetKeyForItem(EntityDefinition item)
            {
                return item.Id;
            }

            /// <summary>
            /// if this key already exists if it does then we need to check
            /// if the existing item is 'older' than the new item and if that is the case we'll replace the older one
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool AddOrUpdate(EntityDefinition item)
            {
                if (Dictionary == null)
                {
                    base.Add(item);
                    return true;
                }

                var key = GetKeyForItem(item);
                EntityDefinition found;
                if (TryGetValue(key, out found))
                {
                    //it already exists and it's older so we need to replace it
                    if (item.VersionId > found.VersionId)
                    {
                        var currIndex = Items.IndexOf(found);
                        if (currIndex == -1)
                            throw new IndexOutOfRangeException("Could not find the item in the list: " + found.Id);

                        //replace the current one with the newer one
                        SetItem(currIndex, item);
                        return true;
                    }
                    //could not add or update
                    return false;
                }

                base.Add(item);
                return true;
            }

            private bool TryGetValue(int key, out EntityDefinition val)
            {
                if (Dictionary == null)
                {
                    val = null;
                    return false;
                }
                return Dictionary.TryGetValue(key, out val);
            }
        }

        private class EntityDefinition
        {
            private readonly UmbracoEntityFactory _factory;
            private readonly dynamic _entity;
            private readonly bool _isContent;
            private readonly bool _isMedia;

            public EntityDefinition(UmbracoEntityFactory factory, dynamic entity, bool isContent, bool isMedia)
            {
                _factory = factory;
                _entity = entity;
                _isContent = isContent;
                _isMedia = isMedia;
            }

            public IUmbracoEntity BuildFromDynamic()
            {
                return _factory.BuildEntityFromDynamic(_entity);
            }

            public int Id
            {
                get { return _entity.id; }
            }

            public int VersionId
            {
                get
                {
                    if (_isContent || _isMedia)
                    {
                        return _entity.versionId;
                    }
                    return _entity.id;
                }
            }
        }
        #endregion
    }
}
