using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    // fixme - use sql templates everywhere!

    /// <summary>
    /// Represents the EntityRepository used to query entity objects.
    /// </summary>
    /// <remarks>
    /// <para>Limited to objects that have a corresponding node (in umbracoNode table).</para>
    /// <para>Returns <see cref="IEntitySlim"/> objects, i.e. lightweight representation of entities.</para>
    /// </remarks>
    internal class EntityRepository : IEntityRepository
    {
        private readonly IScopeAccessor _scopeAccessor;
        private readonly ILanguageRepository _langRepository;

        public EntityRepository(IScopeAccessor scopeAccessor, ILanguageRepository langRepository)
        {
            _scopeAccessor = scopeAccessor;
            _langRepository = langRepository;
        }

        protected IUmbracoDatabase Database => _scopeAccessor.AmbientScope.Database;
        protected Sql<ISqlContext> Sql() => _scopeAccessor.AmbientScope.SqlContext.Sql();

        #region Repository

        // get a page of entities
        public IEnumerable<IEntitySlim> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
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

            //TODO: For isContent will we need to build up the variation info?

            if (isMedia)
                BuildProperties(entities, dtos);

            totalRecords = page.TotalItems;
            return entities;
        }

        public IEntitySlim Get(Guid key)
        {
            var sql = GetBaseWhere(false, false, false, key);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(false, false, dto);
        }

        private IEntitySlim GetEntity(Sql<ISqlContext> sql, bool isContent, bool isMedia)
        {
            //isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var dtos = Database.FetchOneToMany<ContentEntityDto>(
                    ddto => ddto.VariationInfo,
                    ddto => ddto.VersionId,
                    sql);
                return dtos.Count == 0 ? null : BuildDocumentEntity(dtos[0]);
            }

            var dto = Database.FirstOrDefault<BaseDto>(sql);
            if (dto == null) return null;

            var entity = BuildEntity(false, isMedia, dto);

            if (isMedia)
                BuildProperties(entity, dto);

            return entity;
        }

        public IEntitySlim Get(Guid key, Guid objectTypeId)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.Document || objectTypeId == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, key);
            return GetEntity(sql, isContent, isMedia);
        }

        public virtual IEntitySlim Get(int id)
        {
            var sql = GetBaseWhere(false, false, false, id);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(false, false, dto);
        }

        public virtual IEntitySlim Get(int id, Guid objectTypeId)
        {
            var isContent = objectTypeId == Constants.ObjectTypes.Document || objectTypeId == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectTypeId, id);
            return GetEntity(sql, isContent, isMedia);
        }

        public virtual IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids)
        {
            return ids.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAll(objectType);
        }

        public virtual IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys)
        {
            return keys.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAll(objectType);
        }

        private IEnumerable<IEntitySlim> GetEntities(Sql<ISqlContext> sql, bool isContent, bool isMedia)
        {
            //isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var cdtos = Database.FetchOneToMany<ContentEntityDto>(
                    dto => dto.VariationInfo,
                    dto => dto.VersionId,
                    sql);
                return cdtos.Count == 0
                    ? Enumerable.Empty<IEntitySlim>()
                    : cdtos.Select(BuildDocumentEntity).ToArray();
            }

            var dtos = Database.Fetch<BaseDto>(sql);
            if (dtos.Count == 0) return Enumerable.Empty<IEntitySlim>();

            var entities = dtos.Select(x => BuildEntity(false, isMedia, x)).ToArray();

            if (isMedia)
                BuildProperties(entities, dtos);

            return entities;
        }

        private IEnumerable<IEntitySlim> PerformGetAll(Guid objectType, Action<Sql<ISqlContext>> filter = null)
        {
            var isContent = objectType == Constants.ObjectTypes.Document || objectType == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Constants.ObjectTypes.Media;

            var sql = GetFullSqlForEntityType(isContent, isMedia, objectType, filter);
            return GetEntities(sql, isContent, isMedia);
        }

        public virtual IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[] ids)
        {
            return ids.Any()
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAllPaths(objectType);
        }

        public virtual IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAllPaths(objectType);
        }

        private IEnumerable<TreeEntityPath> PerformGetAllPaths(Guid objectType, Action<Sql<ISqlContext>> filter = null)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeId, x => x.Path).From<NodeDto>().Where<NodeDto>(x => x.NodeObjectType == objectType);
            filter?.Invoke(sql);
            return Database.Fetch<TreeEntityPath>(sql);
        }

        public virtual IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBase(false, false, null);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate();
            sql = AddGroupBy(false, false, sql);
            var dtos = Database.Fetch<BaseDto>(sql);
            return dtos.Select(x => BuildEntity(false, false, x)).ToList();
        }

        public virtual IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType)
        {
            var isContent = objectType == Constants.ObjectTypes.Document || objectType == Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Constants.ObjectTypes.Media;

            var sql = GetBaseWhere(isContent, isMedia, false, null, objectType);

            var translator = new SqlTranslator<IUmbracoEntity>(sql, query);
            sql = translator.Translate();
            sql = AddGroupBy(isContent, isMedia, sql);

            return GetEntities(sql, isContent, isMedia);
        }

        public UmbracoObjectTypes GetObjectType(int id)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeObjectType).From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
            return ObjectTypes.GetUmbracoObjectType(Database.ExecuteScalar<Guid>(sql));
        }

        public UmbracoObjectTypes GetObjectType(Guid key)
        {
            var sql = Sql().Select<NodeDto>(x => x.NodeObjectType).From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);
            return ObjectTypes.GetUmbracoObjectType(Database.ExecuteScalar<Guid>(sql));
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

        private void BuildProperties(EntitySlim entity, BaseDto dto)
        {
            var pdtos = Database.Fetch<PropertyDataDto>(GetPropertyData(dto.VersionId));
            foreach (var pdto in pdtos)
                BuildProperty(entity, pdto);
        }

        private void BuildProperties(EntitySlim[] entities, List<BaseDto> dtos)
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

        private void BuildProperty(EntitySlim entity, PropertyDataDto pdto)
        {
            // explain ?!
            var value = string.IsNullOrWhiteSpace(pdto.TextValue)
                ? pdto.VarcharValue
                : pdto.TextValue.ConvertToJsonIfPossible();

            entity.AdditionalData[pdto.PropertyTypeDto.Alias] = new EntitySlim.PropertySlim(pdto.PropertyTypeDto.DataTypeDto.EditorAlias, value);
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
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
                .Where<PropertyDataDto>(x => x.VersionId == versionId);
        }

        private Sql<ISqlContext> GetPropertyData(IEnumerable<int> versionIds)
        {
            return Sql()
                .Select<PropertyDataDto>(r => r.Select(x => x.PropertyTypeDto, r1 => r1.Select(x => x.DataTypeDto)))
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
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


        /// <summary>
        /// The DTO used to fetch results for a content item with its variation info
        /// </summary>
        private class ContentEntityDto : BaseDto
        {
            public ContentVariation Variations { get; set; }

            [ResultColumn, Reference(ReferenceType.Many)]
            public List<ContentEntityVariationInfoDto> VariationInfo { get; set; }

            public bool Published { get; set; }
            public bool Edited { get; set; }
        }

        /// <summary>
        /// The DTO used in the 1:M result for content variation info
        /// </summary>
        private class ContentEntityVariationInfoDto
        {
            [Column("versionCultureId")]
            public int VersionCultureId { get; set; }
            [Column("versionCultureLangId")]
            public int LanguageId { get; set; }
            [Column("versionCultureName")]
            public string Name { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        /// <summary>
        /// the DTO corresponding to fields selected by GetBase
        /// </summary>
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

                if (isContent || isMedia)
                    sql
                        .AndSelect<ContentVersionDto>(x => Alias(x.Id, "versionId"))
                        .AndSelect<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer, x => x.Variations);

                if (isContent)
                {
                    sql
                        .AndSelect<DocumentDto>(x => x.Published, x => x.Edited)
                        //This MUST come last in the select statements since we will end up with a 1:M query
                        .AndSelect<ContentVersionCultureVariationDto>(
                            x => Alias(x.Id, "versionCultureId"),
                            x => Alias(x.LanguageId, "versionCultureLangId"),
                            x => Alias(x.Name, "versionCultureName"));
                }
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

            //Any LeftJoin statements need to come last
            if (isCount == false)
            {
                sql
                    .LeftJoin<NodeDto>("child").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.ParentId, aliasRight: "child");

                if (isContent)
                    sql
                        .LeftJoin<ContentVersionCultureVariationDto>().On<ContentVersionDto, ContentVersionCultureVariationDto>((left, right) => left.Id == right.VersionId);
            }


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
            {
                sql
                    .AndBy<DocumentDto>(x => x.Published, x => x.Edited)
                    .AndBy<ContentVersionCultureVariationDto>(x => x.Id, x => x.LanguageId, x => x.Name);
            }


            if (isContent || isMedia)
                sql
                    .AndBy<ContentVersionDto>(x => x.Id)
                    .AndBy<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer, x => x.Variations);

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

        private EntitySlim BuildEntity(bool isContent, bool isMedia, BaseDto dto)
        {
            if (isContent)
                return BuildDocumentEntity(dto);
            if (isMedia)
                return BuildContentEntity(dto);

            // EntitySlim does not track changes
            var entity = new EntitySlim();
            BuildEntity(entity, dto);
            return entity;
        }

        private static void BuildEntity(EntitySlim entity, BaseDto dto)
        {
            entity.Trashed = dto.Trashed;
            entity.CreateDate = dto.CreateDate;
            entity.CreatorId = dto.UserId ?? 0;
            entity.Id = dto.NodeId;
            entity.Key = dto.UniqueId;
            entity.Level = dto.Level;
            entity.Name = dto.Text;
            entity.NodeObjectType = dto.NodeObjectType;
            entity.ParentId = dto.ParentId;
            entity.Path = dto.Path;
            entity.SortOrder = dto.SortOrder;
            entity.HasChildren = dto.Children > 0;
            entity.IsContainer = dto.IsContainer;
        }

        private static void BuildContentEntity(ContentEntitySlim entity, BaseDto dto)
        {
            BuildEntity(entity, dto);
            entity.ContentTypeAlias = dto.Alias;
            entity.ContentTypeIcon = dto.Icon;
            entity.ContentTypeThumbnail = dto.Thumbnail;
        }

        private static EntitySlim BuildContentEntity(BaseDto dto)
        {
            // EntitySlim does not track changes
            var entity = new ContentEntitySlim();
            BuildContentEntity(entity, dto);
            return entity;
        }

        private DocumentEntitySlim BuildDocumentEntity(BaseDto dto)
        {
            if (dto is ContentEntityDto contentDto)
            {
                return BuildDocumentEntity(contentDto);
            }

            // EntitySlim does not track changes
            var entity = new DocumentEntitySlim();
            BuildContentEntity(entity, dto);
            return entity;
        }

        /// <summary>
        /// Builds the <see cref="EntitySlim"/> from a <see cref="ContentEntityDto"/> and ensures the AdditionalData is populated with variant info
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private DocumentEntitySlim BuildDocumentEntity(ContentEntityDto dto)
        {
            // EntitySlim does not track changes
            var entity = new DocumentEntitySlim();
            BuildContentEntity(entity, dto);
            
            //fixme we need to set these statuses for each variant, see notes in IDocumentEntitySlim
            entity.Edited = dto.Edited;
            entity.Published = dto.Published;

            if (dto.Variations.Has(ContentVariation.CultureNeutral) && dto.VariationInfo != null && dto.VariationInfo.Count > 0)
            {
                var variantInfo = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var info in dto.VariationInfo)
                {
                    var isoCode = _langRepository.GetIsoCodeById(info.LanguageId);
                    if (isoCode != null)
                        variantInfo[isoCode] = info.Name;
                }
                entity.CultureNames = variantInfo;
                entity.Variations = dto.Variations;
            }
            return entity;
        }

        #endregion
    }
}
