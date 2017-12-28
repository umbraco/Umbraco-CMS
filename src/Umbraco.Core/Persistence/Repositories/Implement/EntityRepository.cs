using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    // fixme - use sql templates everywhere!

    /// <summary>
    /// Represents the EntityRepository used to query <see cref="IUmbracoEntity"/> objects.
    /// </summary>
    /// <remarks>
    /// This is limited to objects that are based in the umbracoNode-table.
    /// </remarks>
    internal class EntityRepository : IEntityRepository
    {
        private readonly IScopeAccessor _scopeAccessor;
        public EntityRepository(IScopeAccessor scopeAccessor)
        {
            _scopeAccessor = scopeAccessor;
        }

        protected IUmbracoDatabase Database => _scopeAccessor.AmbientScope.Database;
        protected Sql<ISqlContext> Sql() => _scopeAccessor.AmbientScope.SqlContext.Sql();

        #region Repository

        // get a page of entities
        public IEnumerable<IUmbracoEntity> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, IQuery<IUmbracoEntity> filter = null)
        {
            var isContent = objectType == Constants.ObjectTypes.Document || objectType == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Constants.ObjectTypes.Media;

            var sql = GetBaseWhere(isContent, isMedia, false, x =>
            {
                if (filter == null) return;
                foreach (var filterClause in filter.GetWhereClauses())
                    x.Where(filterClause.Item1, filterClause.Item2);
            }, objectType);

            var translator = new SqlTranslator<IUmbracoEntity>(sql, query);
            sql = translator.Translate();
            sql = AddGroupBy(isContent, isMedia, sql);
            sql = sql.OrderBy<NodeDto>(x => x.NodeId);

            //IEnumerable<IUmbracoEntity> result;
            //
            //if (isMedia)
            //{
            //    //Treat media differently for now, as an Entity it will be returned with ALL of it's properties in the AdditionalData bag!
            //    var pagedResult = UnitOfWork.Database.Page<dynamic>(pageIndex + 1, pageSize, pagedSql);

            //    var ids = pagedResult.Items.Select(x => (int)x.id).InGroupsOf(2000);
            //    var entities = pagedResult.Items.Select(BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();

            //    //Now we need to merge in the property data since we need paging and we can't do this the way that the big media query was working before
            //    foreach (var idGroup in ids)
            //    {
            //        var propSql = GetPropertySql(Constants.ObjectTypes.Media)
            //            .WhereIn<NodeDto>(x => x.NodeId, idGroup)
            //            .OrderBy<NodeDto>(x => x.NodeId);

            //        //This does NOT fetch all data into memory in a list, this will read
            //        // over the records as a data reader, this is much better for performance and memory,
            //        // but it means that during the reading of this data set, nothing else can be read
            //        // from SQL server otherwise we'll get an exception.
            //        var allPropertyData = UnitOfWork.Database.Query<dynamic>(propSql);

            //        //keep track of the current property data item being enumerated
            //        var propertyDataSetEnumerator = allPropertyData.GetEnumerator();
            //        var hasCurrent = false; // initially there is no enumerator.Current

            //        try
            //        {
            //            //This must be sorted by node id (which is done by SQL) because this is how we are sorting the query to lookup property types above,
            //            // which allows us to more efficiently iterate over the large data set of property values.
            //            foreach (var entity in entities)
            //            {
            //                // assemble the dtos for this def
            //                // use the available enumerator.Current if any else move to next
            //                while (hasCurrent || propertyDataSetEnumerator.MoveNext())
            //                {
            //                    if (propertyDataSetEnumerator.Current.nodeId == entity.Id)
            //                    {
            //                        hasCurrent = false; // enumerator.Current is not available

            //                        //the property data goes into the additional data
            //                        entity.AdditionalData[propertyDataSetEnumerator.Current.propertyTypeAlias] = new UmbracoEntity.EntityProperty
            //                        {
            //                            PropertyEditorAlias = propertyDataSetEnumerator.Current.propertyEditorAlias,
            //                            Value = StringExtensions.IsNullOrWhiteSpace(propertyDataSetEnumerator.Current.textValue)
            //                                ? propertyDataSetEnumerator.Current.varcharValue
            //                                : StringExtensions.ConvertToJsonIfPossible(propertyDataSetEnumerator.Current.textValue)
            //                        };
            //                    }
            //                    else
            //                    {
            //                        hasCurrent = true; // enumerator.Current is available for another def
            //                        break; // no more propertyDataDto for this def
            //                    }
            //                }
            //            }
            //        }
            //        finally
            //        {
            //            propertyDataSetEnumerator.Dispose();
            //        }
            //    }

            //    result = entities;
            //}
            //else
            //{
            //    var pagedResult = UnitOfWork.Database.Page<dynamic>(pageIndex + 1, pageSize, pagedSql);
            //    result = pagedResult.Items.Select(BuildEntityFromDynamic).Cast<IUmbracoEntity>().ToList();
            //}

            var page = Database.Page<BaseDto>(pageIndex + 1, pageSize, sql);
            var dtos = page.Items;
            var entities = dtos.Select(x => BuildEntity(isContent, isMedia, x)).ToArray();

            if (isMedia)
                BuildProperties(entities, dtos);

            totalRecords = page.TotalItems;
            return entities;
        }

        public IUmbracoEntity GetByKey(Guid key)
        {
            var sql = GetBaseWhere(false, false, false, key);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(false, false, dto);
        }

        public IUmbracoEntity GetByKey(Guid key, Guid objectTypeId)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.Document || objectTypeId == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, key);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            if (dto == null) return null;

            var entity = BuildEntity(isContent, isMedia, dto);

            if (isMedia)
                BuildProperties(entity, dto);

            return entity;
        }

        public virtual IUmbracoEntity Get(int id)
        {
            var sql = GetBaseWhere(false, false, false, id);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(false, false, dto);
        }

        public virtual IUmbracoEntity Get(int id, Guid objectTypeId)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.Document || objectTypeId == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, id);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            if (dto == null) return null;

            var entity = BuildEntity(isContent, isMedia, dto);

            if (isMedia)
                BuildProperties(entity, dto);

            return entity;
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectType, params int[] ids)
        {
            return ids.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAll(objectType);
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectType, params Guid[] keys)
        {
            return keys.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAll(objectType);
        }

        private IEnumerable<IUmbracoEntity> PerformGetAll(Guid objectType, Action<Sql<ISqlContext>> filter = null)
        {
            var isContent = objectType == Constants.ObjectTypes.Document || objectType == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectType, filter);
            var dtos = Database.Fetch<BaseDto>(sql);
            if (dtos.Count == 0) return Enumerable.Empty<IUmbracoEntity>();

            var entities = dtos.Select(x => BuildEntity(isContent, isMedia, x)).ToArray();

            if (isMedia)
                BuildProperties(entities, dtos);

            return entities;
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(Guid objectType, params int[] ids)
        {
            return ids.Any()
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAllPaths(objectType);
        }

        public virtual IEnumerable<EntityPath> GetAllPaths(Guid objectType, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAllPaths(objectType);
        }

        private IEnumerable<EntityPath> PerformGetAllPaths(Guid objectType, Action<Sql<ISqlContext>> filter = null)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeId, x => x.Path).From<NodeDto>().Where<NodeDto>(x => x.NodeObjectType == objectType);
            filter?.Invoke(sql);
            return Database.Fetch<EntityPath>(sql);
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBase(false, false, null);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate();
            sql = AddGroupBy(false, false, sql);
            var dtos = Database.Fetch<BaseDto>(sql);
            return dtos.Select(x => BuildEntity(false, false, x)).ToList();
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType)
        {
            var isContent = objectType == Constants.ObjectTypes.Document || objectType == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Constants.ObjectTypes.Media;

            var sql = GetBaseWhere(isContent, isMedia, false, null, objectType);
            var translator = new SqlTranslator<IUmbracoEntity>(sql, query);
            sql = translator.Translate();
            sql = AddGroupBy(isContent, isMedia, sql);
            var dtos = Database.Fetch<BaseDto>(sql);
            if (dtos.Count == 0) return Enumerable.Empty<IUmbracoEntity>();

            var entities = dtos.Select(x => BuildEntity(isContent, isMedia, x)).ToArray();

            if (isMedia)
                BuildProperties(entities, dtos);

            return entities;
        }

        public UmbracoObjectTypes GetObjectType(int id)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeObjectType).From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
            return UmbracoObjectTypesExtensions.GetUmbracoObjectType(Database.ExecuteScalar<Guid>(sql));
        }

        public UmbracoObjectTypes GetObjectType(Guid key)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeObjectType).From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
            return UmbracoObjectTypesExtensions.GetUmbracoObjectType(Database.ExecuteScalar<Guid>(sql));
        }

        public bool Exists(Guid key)
        {
            var sql = Sql().SelectCount().From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
            return Database.ExecuteScalar<int>(sql) > 0;
        }

        public bool Exists(int id)
        {
            var sql = Sql().SelectCount().From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
            return Database.ExecuteScalar<int>(sql) > 0;
        }

        private void BuildProperties(UmbracoEntity entity, BaseDto dto)
        {
            var pdtos = Database.Fetch<PropertyDataDto>(GetPropertyData(dto.VersionId));
            foreach (var pdto in pdtos)
                BuildProperty(entity, pdto);
        }

        private void BuildProperties(UmbracoEntity[] entities, List<BaseDto> dtos)
        {
            var versionIds = dtos.Select(x => x.VersionId).Distinct().ToArray();
            var pdtos = Database.FetchByGroups<PropertyDataDto, int>(versionIds, 2000, GetPropertyData);

            var xentity = entities.ToDictionary(x => x.Id, x => x); // nodeId -> entity
            var xdto = dtos.ToDictionary(x => x.VersionId, x => x.NodeId); // versionId -> nodeId
            foreach (var pdto in pdtos)
            {
                var nodeId = xdto[pdto.VersionId];
                var entity = xentity[nodeId];
                BuildProperty(entity, pdto);
            }
        }

        private void BuildProperty(UmbracoEntity entity, PropertyDataDto pdto)
        {
            // explain ?!
            var value = string.IsNullOrWhiteSpace(pdto.TextValue)
                ? pdto.VarcharValue
                : pdto.TextValue.ConvertToJsonIfPossible();

            entity.AdditionalData[pdto.PropertyTypeDto.Alias] = new UmbracoEntity.EntityProperty
            {
                PropertyEditorAlias = pdto.PropertyTypeDto.DataTypeDto.PropertyEditorAlias,
                Value = value
            };
        }

        #endregion

        #region Sql

        // gets the full sql for a given object type and a given unique id
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, Guid objectType, Guid uniqueId)
        {
            var sql = GetBaseWhere(isContent, isMedia, false, objectType, uniqueId);
            return AddGroupBy(isContent, isMedia, sql);
        }

        // gets the full sql for a given object type and a given node id
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, Guid objectType, int nodeId)
        {
            var sql = GetBaseWhere(isContent, isMedia, false, objectType, nodeId);
            return AddGroupBy(isContent, isMedia, sql);
        }

        // gets the full sql for a given object type, with a given filter
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, Guid objectType, Action<Sql<ISqlContext>> filter)
        {
            var sql = GetBaseWhere(isContent, isMedia, false, filter, objectType);
            return AddGroupBy(isContent, isMedia, sql);
        }

        // fixme kill this nonsense
        //// gets the SELECT + FROM + WHERE sql
        //// to get all property data for all items of the specified object type
        //private Sql<ISqlContext> GetPropertySql(Guid objectType)
        //{
        //    return Sql()
        //        .Select<PropertyDataDto>(x => x.VersionId, x => x.TextValue, x => x.VarcharValue)
        //        .AndSelect<NodeDto>(x => x.NodeId)
        //        .AndSelect<DataTypeDto>(x => x.PropertyEditorAlias)
        //        .AndSelect<PropertyTypeDto>(x => Alias(x.Alias, "propertyTypeAlias"))
        //        .From<PropertyDataDto>()
        //        .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((left, right) => left.VersionId == right.Id)
        //        .InnerJoin<NodeDto>().On<ContentVersionDto, NodeDto>((left, right) => left.NodeId == right.NodeId)
        //        .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>(dto => dto.PropertyTypeId, dto => dto.Id)
        //        .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>(dto => dto.DataTypeId, dto => dto.DataTypeId)
        //        .Where<NodeDto>(x => x.NodeObjectType == objectType);
        //}

        private Sql<ISqlContext> GetPropertyData(int versionId)
        {
            return Sql()
                .Select<PropertyDataDto>(r => r.Select(x => x.PropertyTypeDto, r1 => r1.Select(x => x.DataTypeDto)))
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.DataTypeId)
                .Where<PropertyDataDto>(x => x.VersionId == versionId);
        }

        private Sql<ISqlContext> GetPropertyData(IEnumerable<int> versionIds)
        {
            return Sql()
                .Select<PropertyDataDto>(r => r.Select(x => x.PropertyTypeDto, r1 => r1.Select(x => x.DataTypeDto)))
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.DataTypeId)
                .WhereIn<PropertyDataDto>(x => x.VersionId, versionIds)
                .OrderBy<PropertyDataDto>(x => x.VersionId);
        }

        // fixme - wtf is this?
        //private Sql<ISqlContext> GetFullSqlForMedia(Sql<ISqlContext> entitySql, Action<Sql<ISqlContext>> filter = null)
        //{
        //    //this will add any varcharValue property to the output which can be added to the additional properties

        //    var sql = GetPropertySql(Constants.ObjectTypes.Media);

        //    filter?.Invoke(sql);

        //    // We're going to create a query to query against the entity SQL
        //    // because we cannot group by nText columns and we have a COUNT in the entitySql we cannot simply left join
        //    // the entitySql query, we have to join the wrapped query to get the ntext in the result

        //    var wrappedSql = Sql()
        //        .Append("SELECT * FROM (")
        //        .Append(entitySql)
        //        .Append(") tmpTbl LEFT JOIN (")
        //        .Append(sql)
        //        .Append(") as property ON id = property.nodeId")
        //        .OrderBy("sortOrder, id");

        //    return wrappedSql;
        //}

        // the DTO corresponding to fields selected by GetBase
        // ReSharper disable once ClassNeverInstantiated.Local
        private class BaseDto
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // ReSharper disable UnusedMember.Local
            public int NodeId { get; set; }
            public bool Trashed { get; set; }
            public int ParentId { get; set; }
            public int? UserId { get; set; }
            public int Level { get; set; }
            public string Path { get; set; }
            public int SortOrder { get; set; }
            public Guid UniqueId { get; set; }
            public string Text { get; set; }
            public Guid NodeObjectType { get; set; }
            public DateTime CreateDate { get; set; }
            public int Children { get; set; }
            public int VersionId { get; set; }
            public bool Published { get; set; }
            public bool Edited { get; set; }
            public string Alias { get; set; }
            public string Icon { get; set; }
            public string Thumbnail { get; set; }
            public bool IsContainer { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore UnusedMember.Local
        }

        // gets the base SELECT + FROM [+ filter] sql
        // always from the 'current' content version
        protected virtual Sql<ISqlContext> GetBase(bool isContent, bool isMedia, Action<Sql<ISqlContext>> filter, bool isCount = false)
        {
            var sql = Sql();

            if (isCount)
            {
                sql.SelectCount();
            }
            else
            {
                sql
                    .Select<NodeDto>(x => x.NodeId, x => x.Trashed, x => x.ParentId, x => x.UserId, x => x.Level, x => x.Path)
                    .AndSelect<NodeDto>(x => x.SortOrder, x => x.UniqueId, x => x.Text, x => x.NodeObjectType, x => x.CreateDate)
                    .Append(", COUNT(child.id) AS children");

                if (isContent)
                    sql
                        .AndSelect<DocumentDto>(x => x.Published, x => x.Edited);

                if (isContent || isMedia)
                    sql
                        .AndSelect<ContentVersionDto>(x => NPocoSqlExtensions.Statics.Alias(x.Id, "versionId"))
                        .AndSelect<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer);
            }

            sql
                .From<NodeDto>();

            if (isContent || isMedia)
            {
                sql
                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .LeftJoin<ContentTypeDto>().On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId);
            }

            if (isContent)
            {
                sql
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId);
            }

            if (isCount == false)
                sql
                    .LeftJoin<NodeDto>("child").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.ParentId, aliasRight: "child");

            filter?.Invoke(sql);

            return sql;
        }

        // gets the base SELECT + FROM [+ filter] + WHERE sql
        // for a given object type, with a given filter
        protected virtual Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isCount, Action<Sql<ISqlContext>> filter, Guid objectType)
        {
            return GetBase(isContent, isMedia, filter, isCount)
                .Where<NodeDto>(x => x.NodeObjectType == objectType);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given node id
        protected virtual Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isCount, int id)
        {
            var sql = GetBase(isContent, isMedia, null, isCount)
                .Where<NodeDto>(x => x.NodeId == id);
            return AddGroupBy(isContent, isMedia, sql);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given unique id
        protected virtual Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isCount, Guid uniqueId)
        {
            var sql = GetBase(isContent, isMedia, null, isCount)
                .Where<NodeDto>(x => x.UniqueId == uniqueId);
            return AddGroupBy(isContent, isMedia, sql);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given object type and node id
        protected virtual Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isCount, Guid objectType, int nodeId)
        {
            return GetBase(isContent, isMedia, null, isCount)
                .Where<NodeDto>(x => x.NodeId == nodeId && x.NodeObjectType == objectType);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given object type and unique id
        protected virtual Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isCount, Guid objectType, Guid uniqueId)
        {
            return GetBase(isContent, isMedia, null, isCount)
                .Where<NodeDto>(x => x.UniqueId == uniqueId && x.NodeObjectType == objectType);
        }

        // gets the GROUP BY / ORDER BY sql
        // required in order to count children
        protected virtual Sql<ISqlContext> AddGroupBy(bool isContent, bool isMedia, Sql<ISqlContext> sql, bool sort = true)
        {
            sql
                .GroupBy<NodeDto>(x => x.NodeId, x => x.Trashed, x => x.ParentId, x => x.UserId, x => x.Level, x => x.Path)
                .AndBy<NodeDto>(x => x.SortOrder, x => x.UniqueId, x => x.Text, x => x.NodeObjectType, x => x.CreateDate);

            if (isContent)
                sql
                    .AndBy<DocumentDto>(x => x.Published, x => x.Edited);

            if (isContent || isMedia)
                sql
                    .AndBy<ContentVersionDto>(x => x.Id)
                    .AndBy<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer);

            if (sort)
                sql.OrderBy<NodeDto>(x => x.SortOrder);

            return sql;
        }

        #endregion

        #region Classes

        [ExplicitColumns]
        internal class UmbracoPropertyDto
        {
            [Column("propertyEditorAlias")]
            public string PropertyEditorAlias { get; set; }

            [Column("propertyTypeAlias")]
            public string PropertyAlias { get; set; }

            [Column("varcharValue")]
            public string VarcharValue { get; set; }

            [Column("textValue")]
            public string TextValue { get; set; }
        }

        // fixme kill
        /// <summary>
        /// This is a special relator in that it is not returning a DTO but a real resolved entity and that it accepts
        /// a dynamic instance.
        /// </summary>
        /// <remarks>
        /// We're doing this because when we query the db, we want to use dynamic so that it returns all available fields not just the ones
        ///     defined on the entity so we can them to additional data
        /// </remarks>
        //internal class UmbracoEntityRelator
        //{
        //    internal UmbracoEntity Current;

        //    public IEnumerable<IUmbracoEntity> MapAll(IEnumerable<dynamic> input)
        //    {
        //        UmbracoEntity entity;

        //        foreach (var x in input)
        //        {
        //            entity = Map(x);
        //            if (entity != null) yield return entity;
        //        }

        //        entity = Map((dynamic) null);
        //        if (entity != null) yield return entity;
        //    }

        //    // must be called one last time with null in order to return the last one!
        //    public UmbracoEntity Map(dynamic a)
        //    {
        //        // Terminating call.  Since we can return null from this function
        //        // we need to be ready for NPoco to callback later with null
        //        // parameters
        //        if (a == null)
        //            return Current;

        //        string pPropertyEditorAlias = a.propertyEditorAlias;
        //        var pExists = pPropertyEditorAlias != null;
        //        string pPropertyAlias = a.propertyTypeAlias;
        //        string pTextValue = a.textValue;
        //        string pNVarcharValue = a.varcharValue;

        //        // Is this the same UmbracoEntity as the current one we're processing
        //        if (Current != null && Current.Key == a.uniqueID)
        //        {
        //            if (pExists && pPropertyAlias.IsNullOrWhiteSpace() == false)
        //            {
        //                // Add this UmbracoProperty to the current additional data
        //                Current.AdditionalData[pPropertyAlias] = new UmbracoEntity.EntityProperty
        //                {
        //                    PropertyEditorAlias = pPropertyEditorAlias,
        //                    Value = pTextValue.IsNullOrWhiteSpace()
        //                        ? pNVarcharValue
        //                        : pTextValue.ConvertToJsonIfPossible()
        //                };
        //            }

        //            // Return null to indicate we're not done with this UmbracoEntity yet
        //            return null;
        //        }

        //        // This is a different UmbracoEntity to the current one, or this is the
        //        // first time through and we don't have a Tab yet

        //        // Save the current UmbracoEntityDto
        //        var prev = Current;

        //        // Setup the new current UmbracoEntity

        //        Current = BuildEntityFromDynamic(a);

        //        if (pExists && pPropertyAlias.IsNullOrWhiteSpace() == false)
        //        {
        //            //add the property/create the prop list if null
        //            Current.AdditionalData[pPropertyAlias] = new UmbracoEntity.EntityProperty
        //            {
        //                PropertyEditorAlias = pPropertyEditorAlias,
        //                Value = pTextValue.IsNullOrWhiteSpace()
        //                    ? pNVarcharValue
        //                    : pTextValue.ConvertToJsonIfPossible()
        //            };
        //        }

        //        // Return the now populated previous UmbracoEntity (or null if first time through)
        //        return prev;
        //    }
        //}

        // fixme need to review what's below
        // comes from 7.6, similar to what's in VersionableRepositoryBase
        // not sure it really makes sense...

        //private class EntityDefinitionCollection : KeyedCollection<int, EntityDefinition>
        //{
        //    protected override int GetKeyForItem(EntityDefinition item)
        //    {
        //        return item.Id;
        //    }

        //    /// <summary>
        //    /// if this key already exists if it does then we need to check
        //    /// if the existing item is 'older' than the new item and if that is the case we'll replace the older one
        //    /// </summary>
        //    /// <param name="item"></param>
        //    /// <returns></returns>
        //    public bool AddOrUpdate(EntityDefinition item)
        //    {
        //        if (Dictionary == null)
        //        {
        //            Add(item);
        //            return true;
        //        }

        //        var key = GetKeyForItem(item);
        //        if (TryGetValue(key, out EntityDefinition found))
        //        {
        //            //it already exists and it's older so we need to replace it
        //            if (item.VersionId > found.VersionId)
        //            {
        //                var currIndex = Items.IndexOf(found);
        //                if (currIndex == -1)
        //                    throw new IndexOutOfRangeException("Could not find the item in the list: " + found.Id);

        //                //replace the current one with the newer one
        //                SetItem(currIndex, item);
        //                return true;
        //            }
        //            //could not add or update
        //            return false;
        //        }

        //        Add(item);
        //        return true;
        //    }

        //    private bool TryGetValue(int key, out EntityDefinition val)
        //    {
        //        if (Dictionary != null) return Dictionary.TryGetValue(key, out val);

        //        val = null;
        //        return false;
        //    }
        //}

        // fixme wtf is this, why dynamics here, this is horrible !!
        //private class EntityDefinition
        //{
        //    private readonly dynamic _entity;
        //    private readonly bool _isContent;
        //    private readonly bool _isMedia;

        //    public EntityDefinition(dynamic entity, bool isContent, bool isMedia)
        //    {
        //        _entity = entity;
        //        _isContent = isContent;
        //        _isMedia = isMedia;
        //    }

        //    public IUmbracoEntity BuildFromDynamic()
        //    {
        //        return BuildEntityFromDynamic(_entity);
        //    }

        //    public int Id => _entity.id;

        //    public int VersionId
        //    {
        //        get
        //        {
        //            if (_isContent || _isMedia)
        //            {
        //                return _entity.versionId;
        //            }
        //            return _entity.id;
        //        }
        //    }
        //}

        #endregion

        #region Factory

        // fixme kill all this
        //private static void AddAdditionalData(UmbracoEntity entity, IDictionary<string, object> originalEntityProperties)
        //{
        //    var entityProps = typeof(IUmbracoEntity).GetPublicProperties().Select(x => x.Name).ToArray();

        //    // figure out what extra properties we have that are not on the IUmbracoEntity and add them to additional data
        //    foreach (var k in originalEntityProperties.Keys
        //        .Select(x => new { orig = x, title = x.ToCleanString(CleanStringType.PascalCase | CleanStringType.Ascii | CleanStringType.ConvertCase) })
        //        .Where(x => entityProps.InvariantContains(x.title) == false))
        //    {
        //        entity.AdditionalData[k.title] = originalEntityProperties[k.orig];
        //    }
        //}

        //private static UmbracoEntity BuildEntityFromDynamic(dynamic d)
        //{
        //    var asDictionary = (IDictionary<string, object>) d;
        //    var entity = new UmbracoEntity(d.trashed);

        //    try
        //    {
        //        entity.DisableChangeTracking();

        //        entity.CreateDate = d.createDate;
        //        entity.CreatorId = d.nodeUser == null ? 0 : d.nodeUser;
        //        entity.Id = d.id;
        //        entity.Key = d.uniqueID;
        //        entity.Level = d.level;
        //        entity.Name = d.text;
        //        entity.NodeObjectTypeId = d.nodeObjectType;
        //        entity.ParentId = d.parentID;
        //        entity.Path = d.path;
        //        entity.SortOrder = d.sortOrder;
        //        entity.HasChildren = d.children > 0;

        //        entity.ContentTypeAlias = asDictionary.ContainsKey("alias") ? (d.alias ?? string.Empty) : string.Empty;
        //        entity.ContentTypeIcon = asDictionary.ContainsKey("icon") ? (d.icon ?? string.Empty) : string.Empty;
        //        entity.ContentTypeThumbnail = asDictionary.ContainsKey("thumbnail") ? (d.thumbnail ?? string.Empty) : string.Empty;
        //        //entity.VersionId = asDictionary.ContainsKey("versionId") ? asDictionary["versionId"] : Guid.Empty;

        //        entity.Published = asDictionary.ContainsKey("published") && (bool) asDictionary["published"];
        //        entity.Edited = asDictionary.ContainsKey("edited") && (bool) asDictionary["edited"];

        //        // assign the additional data
        //        AddAdditionalData(entity, asDictionary);

        //        return entity;
        //    }
        //    finally
        //    {
        //        entity.EnableChangeTracking();
        //    }
        //}

        private static UmbracoEntity BuildEntity(bool isContent, bool isMedia, BaseDto dto)
        {
            var entity = new UmbracoEntity(dto.Trashed);

            try
            {
                entity.DisableChangeTracking();

                entity.CreateDate = dto.CreateDate;
                entity.CreatorId = dto.UserId ?? 0;
                entity.Id = dto.NodeId;
                entity.Key = dto.UniqueId;
                entity.Level = dto.Level;
                entity.Name = dto.Text;
                entity.NodeObjectTypeId = dto.NodeObjectType;
                entity.ParentId = dto.ParentId;
                entity.Path = dto.Path;
                entity.SortOrder = dto.SortOrder;
                entity.HasChildren = dto.Children > 0;

                if (isContent)
                {
                    entity.Published = dto.Published;
                    entity.Edited = dto.Edited;
                }

                // fixme what shall we do with versions?
                //entity.VersionId = asDictionary.ContainsKey("versionId") ? asDictionary["versionId"] : Guid.Empty;

                if (isContent || isMedia)
                {
                    entity.ContentTypeAlias = dto.Alias;
                    entity.ContentTypeIcon = dto.Icon;
                    entity.ContentTypeThumbnail = dto.Thumbnail;
                    //entity.??? = dto.IsContainer;
                }
            }
            finally
            {
                entity.EnableChangeTracking();
            }

            return entity;
        }

        #endregion
    }
}
