using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NPoco;
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
    internal class EntityRepository : DisposableObject, IEntityRepository
    {
        public EntityRepository(IScopeUnitOfWork work)
        {
            UnitOfWork = work;
        }

        /// <summary>
        /// Gets the repository's unit of work.
        /// </summary>
        protected internal IScopeUnitOfWork UnitOfWork { get; }

        #region Query Methods

        public IQuery<IUmbracoEntity> Query => UnitOfWork.Query<IUmbracoEntity>();

        public Sql<SqlContext> Sql() => UnitOfWork.Sql();

        // fixme need to review that ... one
        public IEnumerable<IUmbracoEntity> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, IQuery<IUmbracoEntity> filter = null)
        {
            var isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var factory = new UmbracoEntityFactory();

            var sqlClause = GetBaseWhere(GetBase, isContent, isMedia, sql =>
            {
                if (filter != null)
                {
                    foreach (var filterClaus in filter.GetWhereClauses())
                    {
                        sql.Where(filterClaus.Item1, filterClaus.Item2);
                    }
                }
            }, objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var entitySql = translator.Translate();
            var pagedSql = entitySql.Append(GetGroupBy(isContent, isMedia, false)).OrderBy("umbracoNode.id");

            IEnumerable<IUmbracoEntity> result;

            if (isMedia)
            {
                //Treat media differently for now, as an Entity it will be returned with ALL of it's properties in the AdditionalData bag!
                var pagedResult = UnitOfWork.Database.Page<dynamic>(pageIndex + 1, pageSize, pagedSql);

                var ids = pagedResult.Items.Select(x => (int)x.id).InGroupsOf(2000);
                var entities = pagedResult.Items.Select(factory.BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();

                //Now we need to merge in the property data since we need paging and we can't do this the way that the big media query was working before
                foreach (var idGroup in ids)
                {
                    var propSql = GetPropertySql(Constants.ObjectTypes.Media)
                        .Where("contentNodeId IN (@ids)", new { ids = idGroup })
                        .OrderBy("contentNodeId");

                    //This does NOT fetch all data into memory in a list, this will read
                    // over the records as a data reader, this is much better for performance and memory,
                    // but it means that during the reading of this data set, nothing else can be read
                    // from SQL server otherwise we'll get an exception.
                    var allPropertyData = UnitOfWork.Database.Query<dynamic>(propSql);

                    //keep track of the current property data item being enumerated
                    var propertyDataSetEnumerator = allPropertyData.GetEnumerator();
                    var hasCurrent = false; // initially there is no enumerator.Current

                    try
                    {
                        //This must be sorted by node id (which is done by SQL) because this is how we are sorting the query to lookup property types above,
                        // which allows us to more efficiently iterate over the large data set of property values.
                        foreach (var entity in entities)
                        {
                            // assemble the dtos for this def
                            // use the available enumerator.Current if any else move to next
                            while (hasCurrent || propertyDataSetEnumerator.MoveNext())
                            {
                                if (propertyDataSetEnumerator.Current.contentNodeId == entity.Id)
                                {
                                    hasCurrent = false; // enumerator.Current is not available

                                    //the property data goes into the additional data
                                    entity.AdditionalData[propertyDataSetEnumerator.Current.propertyTypeAlias] = new UmbracoEntity.EntityProperty
                                    {
                                        PropertyEditorAlias = propertyDataSetEnumerator.Current.propertyEditorAlias,
                                        Value = StringExtensions.IsNullOrWhiteSpace(propertyDataSetEnumerator.Current.dataNtext)
                                            ? propertyDataSetEnumerator.Current.dataNvarchar
                                            : StringExtensions.ConvertToJsonIfPossible(propertyDataSetEnumerator.Current.dataNtext)
                                    };
                                }
                                else
                                {
                                    hasCurrent = true; // enumerator.Current is available for another def
                                    break; // no more propertyDataDto for this def
                                }
                            }
                        }
                    }
                    finally
                    {
                        propertyDataSetEnumerator.Dispose();
                    }
                }

                result = entities;
            }
            else
            {
                var pagedResult = UnitOfWork.Database.Page<dynamic>(pageIndex + 1, pageSize, pagedSql);
                result = pagedResult.Items.Select(factory.BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();
            }

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
                        foreach (var filterClaus in filter.GetWhereClauses())
                        {
                            sql.Where(filterClaus.Item1, filterClaus.Item2);
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
            var isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);

            var sql = GetFullSqlForEntityType(key, isContent, isMedia, objectTypeId);

            if (isMedia)
            {
                //for now treat media differently and include all property data too
                return UnitOfWork.Database
                    .Fetch<dynamic>(sql)
                    .Transform(new UmbracoEntityRelator().MapAll)
                    .FirstOrDefault();
            }

            // else

            //query = read forward data reader, do not load everything into mem
            // fixme wtf is this collection thing?!
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            var factory = new UmbracoEntityFactory();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, false));
            }
            var found = collection.FirstOrDefault();
            return found != null ? found.BuildFromDynamic() : null;

            var nodeDto = UnitOfWork.Database.FirstOrDefault<dynamic>(sql);
            if (nodeDto == null)
                return null;

            var entity = factory.BuildEntityFromDynamic(nodeDto);

            return entity;
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
            var isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);

            var sql = GetFullSqlForEntityType(id, isContent, isMedia, objectTypeId);

            if (isMedia)
            {
                //for now treat media differently and include all property data too
                return UnitOfWork.Database
                    .Fetch<dynamic>(sql)
                    .Transform(new UmbracoEntityRelator().MapAll)
                    .FirstOrDefault();
            }

            // else

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            var factory = new UmbracoEntityFactory();
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
                ? PerformGetAll(objectTypeId, sql1 => sql1.Where(" umbracoNode.id in (@ids)", new { /*ids =*/ ids }))
                : PerformGetAll(objectTypeId);
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAll(objectTypeId, sql1 => sql1.Where(" umbracoNode.uniqueID in (@keys)", new { /*keys =*/ keys }))
                : PerformGetAll(objectTypeId);
        }

        private IEnumerable<IUmbracoEntity> PerformGetAll(Guid objectTypeId, Action<Sql> filter = null)
        {
            var isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, filter);

            if (isMedia)
            {
                //for now treat media differently  and include all property data too
                return UnitOfWork.Database
                    .Fetch<dynamic>(sql)
                    .Transform(new UmbracoEntityRelator().MapAll);
            }

            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(sql);
            var collection = new EntityDefinitionCollection();
            var factory = new UmbracoEntityFactory();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, false));
            }
            return collection.Select(x => x.BuildFromDynamic()).ToList();
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

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            var isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);

            var sqlClause = GetBaseWhere(GetBase, isContent, isMedia, null, objectTypeId);

            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var entitySql = translator.Translate();

            if (isMedia)
            {
                var wheres = query.GetWhereClauses().ToArray();

                var mediaSql = GetFullSqlForMedia(entitySql.Append(GetGroupBy(isContent, true, false)), sql =>
                {
                    //adds the additional filters
                    foreach (var whereClause in wheres)
                    {
                        sql.Where(whereClause.Item1, whereClause.Item2);
                    }
                });

                //for now treat media differently and include all property data too
                return UnitOfWork.Database
                    .Fetch<dynamic>(mediaSql)
                    .Transform(new UmbracoEntityRelator().MapAll);
            }

            // else

            //use dynamic so that we can get ALL properties from the SQL so we can chuck that data into our AdditionalData
            var finalSql = entitySql.Append(GetGroupBy(isContent, false));
            //query = read forward data reader, do not load everything into mem
            var dtos = UnitOfWork.Database.Query<dynamic>(finalSql);
            var collection = new EntityDefinitionCollection();
            var factory = new UmbracoEntityFactory();
            foreach (var dto in dtos)
            {
                collection.AddOrUpdate(new EntityDefinition(factory, dto, isContent, false));
            }
            return collection.Select(x => x.BuildFromDynamic()).ToList();
        }

        public UmbracoObjectTypes GetObjectType(int id)
        {
            var sql = Sql().Select("nodeObjectType").From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
            var nodeObjectTypeId = UnitOfWork.Database.ExecuteScalar<Guid>(sql);
            var objectTypeId = nodeObjectTypeId;
            return UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
        }

        public UmbracoObjectTypes GetObjectType(Guid key)
        {
            var sql = Sql().Select("nodeObjectType").From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
            var nodeObjectTypeId = UnitOfWork.Database.ExecuteScalar<Guid>(sql);
            var objectTypeId = nodeObjectTypeId;
            return UmbracoObjectTypesExtensions.GetUmbracoObjectType(objectTypeId);
        }

        #endregion


        #region Sql Statements

        protected Sql<SqlContext> GetFullSqlForEntityType(Guid key, bool isContent, bool isMedia, Guid objectTypeId)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, key);

            return isMedia
                ? GetFullSqlForMedia(entitySql.Append(GetGroupBy(isContent, true, false)))
                : entitySql.Append(GetGroupBy(isContent, false));
        }

        protected Sql<SqlContext> GetFullSqlForEntityType(int id, bool isContent, bool isMedia, Guid objectTypeId)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, id);

            return isMedia
                ? GetFullSqlForMedia(entitySql.Append(GetGroupBy(isContent, true, false)))
                : entitySql.Append(GetGroupBy(isContent, false));
        }

        protected Sql<SqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, Guid objectTypeId, Action<Sql<SqlContext>> filter)
        {
            var entitySql = GetBaseWhere(GetBase, isContent, isMedia, filter, objectTypeId);

            return isMedia
                ? GetFullSqlForMedia(entitySql.Append(GetGroupBy(isContent, true, false)), filter)
                : entitySql.Append(GetGroupBy(isContent, false));
        }

        private Sql<SqlContext> GetPropertySql(string nodeObjectType)
        {
            var sql = Sql()
                .Select("contentNodeId, versionId, dataNvarchar, dataNtext, propertyEditorAlias, alias as propertyTypeAlias")
                .From<PropertyDataDto>()
                .InnerJoin<NodeDto>()
                .On<PropertyDataDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, PropertyDataDto>(dto => dto.Id, dto => dto.PropertyTypeId)
                .InnerJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(dto => dto.DataTypeId, dto => dto.DataTypeId)
                .Where("umbracoNode.nodeObjectType = @nodeObjectType", new { nodeObjectType });

            return sql;
        }

        private Sql<SqlContext> GetFullSqlForMedia(Sql<SqlContext> entitySql, Action<Sql<SqlContext>> filter = null)
        {
            //this will add any dataNvarchar property to the output which can be added to the additional properties

            var sql = GetPropertySql(Constants.ObjectTypes.Media);

            filter?.Invoke(sql);

            //We're going to create a query to query against the entity SQL
            // because we cannot group by nText columns and we have a COUNT in the entitySql we cannot simply left join
            // the entitySql query, we have to join the wrapped query to get the ntext in the result

            var wrappedSql = Sql()
                .Append("SELECT * FROM (")
                .Append(entitySql)
                .Append(") tmpTbl LEFT JOIN (")
                .Append(sql)
                .Append(") as property ON id = property.contentNodeId")
                .OrderBy("sortOrder, id");

            return wrappedSql;
        }

        protected virtual Sql<SqlContext> GetBase(bool isContent, bool isMedia, Action<Sql<SqlContext>> customFilter)
        {
            return GetBase(isContent, isMedia, customFilter, false);
        }

        protected virtual Sql<SqlContext> GetBase(bool isContent, bool isMedia, Action<Sql<SqlContext>> customFilter, bool isCount)
        {
            var columns = new List<object>();

            if (isCount)
            {
                columns.Add("COUNT(*)");
            }
            else
            {
                columns.AddRange(new[]
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

                if (isContent)
                {
                    columns.Add("published.versionId as publishedVersion");
                    columns.Add("document.versionId as newestVersion");
                    columns.Add("contentversion.id as versionId");
                }

                if (isContent || isMedia)
                {
                    columns.Add("contenttype.alias");
                    columns.Add("contenttype.icon");
                    columns.Add("contenttype.thumbnail");
                    columns.Add("contenttype.isContainer");
                }
            }

            //Creates an SQL query to return a single row for the entity

            var entitySql = Sql()
                .Select(columns.ToArray())
                .From("umbracoNode umbracoNode");

            if (isContent || isMedia)
            {
                entitySql
                    .InnerJoin("cmsContent content").On("content.nodeId = umbracoNode.id");
            }

            if (isContent)
            {
                entitySql
                    .InnerJoin("cmsDocument document").On("document.nodeId = umbracoNode.id")
                    .InnerJoin("cmsContentVersion contentversion").On("contentversion.VersionId = document.versionId")
                    .LeftJoin("(SELECT nodeId, versionId FROM cmsDocument WHERE published = 1 GROUP BY nodeId, versionId) as published").On("umbracoNode.id = published.nodeId");
                    //.LeftJoin("(SELECT nodeId, versionId FROM cmsDocument WHERE newest = 1 GROUP BY nodeId, versionId) as latest").On("umbracoNode.id = latest.nodeId");
            }

            if (isContent || isMedia)
            {
                entitySql
                   .LeftJoin("cmsContentType contenttype").On("contenttype.nodeId = content.contentType");
            }

            if (isCount == false)
                entitySql.LeftJoin("umbracoNode parent").On("parent.parentID = umbracoNode.id");

            customFilter?.Invoke(entitySql);

            return entitySql;
        }

        protected virtual Sql<SqlContext> GetBaseWhere(Func<bool, bool, Action<Sql<SqlContext>>, Sql<SqlContext>> baseQuery, bool isContent, bool isMedia, Action<Sql<SqlContext>> filter, Guid nodeObjectType)
        {
            var sql = baseQuery(isContent, isMedia, filter)
                .Where("umbracoNode.nodeObjectType = @nodeObjectType", new { nodeObjectType });
            if (isContent)
                sql.Where("document.newest = 1");
            return sql;
        }

        protected virtual Sql<SqlContext> GetBaseWhere(Func<bool, bool, Action<Sql<SqlContext>>, Sql<SqlContext>> baseQuery, bool isContent, bool isMedia, int id)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.id = @id", new { id });
            if (isContent)
                sql.Where("document.newest = 1");
            sql.Append(GetGroupBy(isContent, isMedia));
            return sql;
        }

        protected virtual Sql<SqlContext> GetBaseWhere(Func<bool, bool, Action<Sql<SqlContext>>, Sql<SqlContext>> baseQuery, bool isContent, bool isMedia, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.uniqueID = @uniqueID", new { uniqueID = key });
            if (isContent)
                sql.Where("document.newest = 1");
            sql.Append(GetGroupBy(isContent, isMedia));
            return sql;
        }

        protected virtual Sql<SqlContext> GetBaseWhere(Func<bool, bool, Action<Sql<SqlContext>>, Sql<SqlContext>> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, int id)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.id = @id AND umbracoNode.nodeObjectType = @nodeObjectType", new { id, nodeObjectType});
            if (isContent)
                sql.Where("document.newest = 1");
            return sql;
        }

        protected virtual Sql<SqlContext> GetBaseWhere(Func<bool, bool, Action<Sql<SqlContext>>, Sql<SqlContext>> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, null)
                .Where("umbracoNode.uniqueID = @uniqueID AND umbracoNode.nodeObjectType = @nodeObjectType", new { uniqueID = key, nodeObjectType });
            if (isContent)
                sql.Where("document.newest = 1");
            return sql;
        }

        protected virtual Sql<SqlContext> GetGroupBy(bool isContent, bool isMedia, bool includeSort = true)
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

            if (isContent)
            {
                columns.Add("published.versionId");
                columns.Add("document.versionId");
                columns.Add("contentVersion.id");
            }

            if (isContent || isMedia)
            {
                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("contenttype.isContainer");
            }

            var sql = Sql().GroupBy(columns.ToArray());

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
            var sql = new Sql().Select("COUNT(*)").From("umbracoNode").Where("uniqueID=@uniqueID", new { uniqueID = key });
            return UnitOfWork.Database.ExecuteScalar<int>(sql) > 0;
        }

        public bool Exists(int id)
        {
            var sql = new Sql().Select("COUNT(*)").From("umbracoNode").Where("id=@id", new { id });
            return UnitOfWork.Database.ExecuteScalar<int>(sql) > 0;
        }

        #region Classes

        [ExplicitColumns]
        internal class UmbracoPropertyDto
        {
            [Column("propertyEditorAlias")]
            public string PropertyEditorAlias { get; set; }

            [Column("propertyTypeAlias")]
            public string PropertyAlias { get; set; }

            [Column("dataNvarchar")]
            public string NVarcharValue { get; set; }

            [Column("dataNtext")]
            public string NTextValue { get; set; }
        }

        /// <summary>
        /// This is a special relator in that it is not returning a DTO but a real resolved entity and that it accepts
        /// a dynamic instance.
        /// </summary>
        /// <remarks>
        /// We're doing this because when we query the db, we want to use dynamic so that it returns all available fields not just the ones
        ///     defined on the entity so we can them to additional data
        /// </remarks>
        internal class UmbracoEntityRelator
        {
            internal UmbracoEntity Current;
            private readonly UmbracoEntityFactory _factory = new UmbracoEntityFactory();

            public IEnumerable<IUmbracoEntity> MapAll(IEnumerable<dynamic> input)
            {
                UmbracoEntity entity;

                foreach (var x in input)
                {
                    entity = Map(x);
                    if (entity != null) yield return entity;
                }

                entity = Map((dynamic) null);
                if (entity != null) yield return entity;
            }

            // must be called one last time with null in order to return the last one!
            public UmbracoEntity Map(dynamic a)
            {
                // Terminating call.  Since we can return null from this function
                // we need to be ready for NPoco to callback later with null
                // parameters
                if (a == null)
                    return Current;

                string pPropertyEditorAlias = a.propertyEditorAlias;
                var pExists = pPropertyEditorAlias != null;
                string pPropertyAlias = a.propertyTypeAlias;
                string pNTextValue = a.dataNtext;
                string pNVarcharValue = a.dataNvarchar;

                // Is this the same UmbracoEntity as the current one we're processing
                if (Current != null && Current.Key == a.uniqueID)
                {
                    if (pExists && pPropertyAlias.IsNullOrWhiteSpace() == false)
                    {
                        // Add this UmbracoProperty to the current additional data
                        Current.AdditionalData[pPropertyAlias] = new UmbracoEntity.EntityProperty
                        {
                            PropertyEditorAlias = pPropertyEditorAlias,
                            Value = pNTextValue.IsNullOrWhiteSpace()
                                ? pNVarcharValue
                                : pNTextValue.ConvertToJsonIfPossible()
                        };
                    }

                    // Return null to indicate we're not done with this UmbracoEntity yet
                    return null;
                }

                // This is a different UmbracoEntity to the current one, or this is the
                // first time through and we don't have a Tab yet

                // Save the current UmbracoEntityDto
                var prev = Current;

                // Setup the new current UmbracoEntity

                Current = _factory.BuildEntityFromDynamic(a);

                if (pExists && pPropertyAlias.IsNullOrWhiteSpace() == false)
                {
                    //add the property/create the prop list if null
                    Current.AdditionalData[pPropertyAlias] = new UmbracoEntity.EntityProperty
                    {
                        PropertyEditorAlias = pPropertyEditorAlias,
                        Value = pNTextValue.IsNullOrWhiteSpace()
                            ? pNVarcharValue
                            : pNTextValue.ConvertToJsonIfPossible()
                    };
                }

                // Return the now populated previous UmbracoEntity (or null if first time through)
                return prev;
            }
        }

        // fixme need to review what's below
        // comes from 7.6, similar to what's in VersionableRepositoryBase
        // not sure it really makes sense...

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
                    Add(item);
                    return true;
                }

                var key = GetKeyForItem(item);
                if (TryGetValue(key, out EntityDefinition found))
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

                Add(item);
                return true;
            }

            private bool TryGetValue(int key, out EntityDefinition val)
            {
                if (Dictionary != null) return Dictionary.TryGetValue(key, out val);

                val = null;
                return false;
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

            public int Id => _entity.id;

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