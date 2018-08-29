using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal sealed class ContentRepositoryBase
    {
        /// <summary>
        /// This is used for unit tests ONLY
        /// </summary>
        public static bool ThrowOnWarning = false;
    }

    internal abstract class ContentRepositoryBase<TId, TEntity, TRepository> : NPocoRepositoryBase<TId, TEntity>, IContentRepository<TId, TEntity>
        where TEntity : class, IUmbracoEntity
        where TRepository : class, IRepository
    {
        protected ContentRepositoryBase(IScopeAccessor scopeAccessor, CacheHelper cache, ILanguageRepository languageRepository, ILogger logger)
            : base(scopeAccessor, cache, logger)
        {
            LanguageRepository = languageRepository;
        }

        protected abstract TRepository This { get; }

        protected ILanguageRepository LanguageRepository { get; }

        protected PropertyEditorCollection PropertyEditors => Current.PropertyEditors; // fixme inject

        #region Versions

        // gets a specific version
        public abstract TEntity GetVersion(int versionId);

        // gets all versions, current first
        public abstract IEnumerable<TEntity> GetAllVersions(int nodeId);

        // gets all version ids, current first
        public virtual IEnumerable<int> GetVersionIds(int nodeId, int maxRows)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersionIds", tsql =>
                tsql.Select<ContentVersionDto>(x => x.Id)
                    .From<ContentVersionDto>()
                    .Where<ContentVersionDto>(x => x.NodeId == SqlTemplate.Arg<int>("nodeId"))
                    .OrderByDescending<ContentVersionDto>(x => x.Current) // current '1' comes before others '0'
                    .AndByDescending<ContentVersionDto>(x => x.VersionDate) // most recent first
            );
            return Database.Fetch<int>(SqlSyntax.SelectTop(template.Sql(nodeId), maxRows));
        }

        // deletes a specific version
        public virtual void DeleteVersion(int versionId)
        {
            // fixme test object node type?

            // get the version we want to delete
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersion", tsql =>
                tsql.Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.Id == SqlTemplate.Arg<int>("versionId"))
            );
            var versionDto = Database.Fetch<ContentVersionDto>(template.Sql(new { versionId })).FirstOrDefault();

            // nothing to delete
            if (versionDto == null)
                return;

            // don't delete the current version
            if (versionDto.Current)
                throw new InvalidOperationException("Cannot delete the current version.");

            PerformDeleteVersion(versionDto.NodeId, versionId);
        }

        //  deletes all versions of an entity, older than a date.
        public virtual void DeleteVersions(int nodeId, DateTime versionDate)
        {
            // fixme test object node type?

            // get the versions we want to delete, excluding the current one
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersions", tsql =>
                tsql.Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.NodeId == SqlTemplate.Arg<int>("nodeId") && !x.Current && x.VersionDate < SqlTemplate.Arg<DateTime>("versionDate"))
            );
            var versionDtos = Database.Fetch<ContentVersionDto>(template.Sql(new { nodeId, versionDate }));
            foreach (var versionDto in versionDtos)
                PerformDeleteVersion(versionDto.NodeId, versionDto.Id);
        }

        // actually deletes a version
        protected abstract void PerformDeleteVersion(int id, int versionId);

        #endregion

        #region Count

        /// <summary>
        /// Count descendants of an item.
        /// </summary>
        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            var pathMatch = parentId == -1
                ? "-1,"
                : "," + parentId + ",";

            var sql = SqlContext.Sql()
                .SelectCount()
                .From<NodeDto>();

            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.Path.Contains(pathMatch));
            }
            else
            {
                sql
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>()
                    .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.Path.Contains(pathMatch))
                    .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias);
            }

            return Database.ExecuteScalar<int>(sql);
        }

        /// <summary>
        /// Count children of an item.
        /// </summary>
        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            var sql = SqlContext.Sql()
                .SelectCount()
                .From<NodeDto>();

            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.ParentId == parentId);
            }
            else
            {
                sql
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>()
                    .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.ParentId == parentId)
                    .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias);
            }

            return Database.ExecuteScalar<int>(sql);
        }

        /// <summary>
        /// Count items.
        /// </summary>
        public int Count(string contentTypeAlias = null)
        {
            var sql = SqlContext.Sql()
                .SelectCount()
                .From<NodeDto>();

            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }
            else
            {
                sql
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>()
                    .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias);
            }

            return Database.ExecuteScalar<int>(sql);
        }

        #endregion

        #region Tags

        /// <summary>
        /// Updates tags for an item.
        /// </summary>
        protected void SetEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            foreach (var property in entity.Properties)
            {
                var tagConfiguration = property.GetTagConfiguration();
                if (tagConfiguration == null) continue;
                tagRepo.Assign(entity.Id, property.PropertyTypeId, property.GetTagsValue().Select(x => new Tag { Group = tagConfiguration.Group, Text = x }), true);
            }
        }

        // FIXME should we do it when un-publishing? or?
        /// <summary>
        /// Clears tags for an item.
        /// </summary>
        protected void ClearEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            tagRepo.RemoveAll(entity.Id);
        }

        #endregion

        public abstract IEnumerable<TEntity> GetPage(IQuery<TEntity> query, long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<TEntity> filter = null);

        // sql: the main sql
        // filterSql: a filtering ? fixme different from v7?
        // orderBy: the name of an ordering field
        // orderDirection: direction for orderBy
        // orderBySystemField: whether orderBy is a system field or a custom field (property value)
        private Sql<ISqlContext> PrepareSqlForPage(Sql<ISqlContext> sql, Sql<ISqlContext> filterSql, string orderBy, Direction orderDirection, bool orderBySystemField)
        {
            if (filterSql == null && string.IsNullOrEmpty(orderBy)) return sql;

            // preserve original
            var psql = new Sql<ISqlContext>(sql.SqlContext, sql.SQL, sql.Arguments);

            // apply filter
            if (filterSql != null)
                psql.Append(filterSql);

            // non-sorting, we're done
            if (string.IsNullOrEmpty(orderBy))
                return psql;

            // else apply sort
            var dbfield = orderBySystemField
                ? GetOrderBySystemField(ref psql, orderBy)
                : GetOrderByNonSystemField(ref psql, orderBy);

            if (orderDirection == Direction.Ascending)
                psql.OrderBy(dbfield);
            else
                psql.OrderByDescending(dbfield);

            // no matter what we always MUST order the result also by umbracoNode.id to ensure that all records being ordered by are unique.
            // if we do not do this then we end up with issues where we are ordering by a field that has duplicate values (i.e. the 'text' column
            // is empty for many nodes) - see: http://issues.umbraco.org/issue/U4-8831

            dbfield = GetDatabaseFieldNameForOrderBy("umbracoNode", "id");
            if (orderBySystemField == false || orderBy.InvariantEquals(dbfield) == false)
            {
                // get alias, if aliased
                var matches = SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
                var match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(dbfield));
                if (match != null) dbfield = match.Groups[2].Value;

                // add field
                psql.OrderBy(dbfield);
            }

            // create prepared sql
            // ensure it's single-line as NPoco PagingHelper has issues with multi-lines
            psql = new Sql<ISqlContext>(psql.SqlContext, psql.SQL.ToSingleLine(), psql.Arguments);
            return psql;
        }

        private string GetOrderBySystemField(ref Sql<ISqlContext> sql, string orderBy)
        {
            // get the database field eg "[table].[column]"
            var dbfield = GetDatabaseFieldNameForOrderBy(orderBy);

            // for SqlServer pagination to work, the "order by" field needs to be the alias eg if
            // the select statement has "umbracoNode.text AS NodeDto__Text" then the order field needs
            // to be "NodeDto__Text" and NOT "umbracoNode.text".
            // not sure about SqlCE nor MySql, so better do it too. initially thought about patching
            // NPoco but that would be expensive and not 100% possible, so better give NPoco proper
            // queries to begin with.
            // thought about maintaining a map of columns-to-aliases in the sql context but that would
            // be expensive and most of the time, useless. so instead we parse the SQL looking for the
            // alias. somewhat expensive too but nothing's free.

            // note: ContentTypeAlias is not properly managed because it's not part of the query to begin with!

            // get alias, if aliased
            var matches = SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
            var match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(dbfield));
            if (match != null) dbfield = match.Groups[2].Value;

            return dbfield;
        }

        private string GetOrderByNonSystemField(ref Sql<ISqlContext> sql, string orderBy)
        {
            // sorting by a custom field, so set-up sub-query for ORDER BY clause to pull through value
            // from 'current' content version for the given order by field
            var sortedInt = string.Format(SqlContext.SqlSyntax.ConvertIntegerToOrderableString, "intValue");
            var sortedDate = string.Format(SqlContext.SqlSyntax.ConvertDateToOrderableString, "dateValue");
            var sortedString = "COALESCE(varcharValue,'')"; // assuming COALESCE is ok for all syntaxes
            var sortedDecimal = string.Format(SqlContext.SqlSyntax.ConvertDecimalToOrderableString, "decimalValue");

            // needs to be an outer join since there's no guarantee that any of the nodes have values for this property
            var innerSql = Sql().Select($@"CASE
                            WHEN intValue IS NOT NULL THEN {sortedInt}
                            WHEN decimalValue IS NOT NULL THEN {sortedDecimal}
                            WHEN dateValue IS NOT NULL THEN {sortedDate}
                            ELSE {sortedString}
                        END AS customPropVal,
                        cver.nodeId AS customPropNodeId")
                .From<ContentVersionDto>("cver")
                .InnerJoin<PropertyDataDto>("opdata").On<ContentVersionDto, PropertyDataDto>((left, right) => left.Id == right.Id, "cver", "opdata")
                .InnerJoin<PropertyTypeDto>("optype").On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id, "opdata", "optype")
                .Where<ContentVersionDto>(x => x.Current, "cver") // always query on current (edit) values
                .Where<PropertyTypeDto>(x => x.Alias == "", "optype");

            // @0 is for x.Current ie 'true' = 1
            // @1 is for x.Alias
            var innerSqlString = innerSql.SQL.Replace("@0", "1").Replace("@1", "@" + sql.Arguments.Length);
            var outerJoinTempTable = $@"LEFT OUTER JOIN ({innerSqlString}) AS customPropData
                ON customPropData.customPropNodeId = {Constants.DatabaseSchema.Tables.Node}.id "; // trailing space is important!

            // insert this just above the last WHERE
            var pos = sql.SQL.InvariantIndexOf("WHERE");
            if (pos < 0) throw new Exception("Oops, WHERE not found.");
            var newSql = sql.SQL.Insert(pos, outerJoinTempTable);

            var newArgs = sql.Arguments.ToList();
            newArgs.Add(orderBy);

            // insert the SQL selected field, too, else ordering cannot work
            if (sql.SQL.StartsWith("SELECT ") == false) throw new Exception("Oops: SELECT not found.");
            newSql = newSql.Insert("SELECT ".Length, "customPropData.customPropVal, ");

            sql = new Sql<ISqlContext>(sql.SqlContext, newSql, newArgs.ToArray());

            // and order by the custom field
            return "customPropData.customPropVal";
        }

        protected IEnumerable<TEntity> GetPage<TDto>(IQuery<TEntity> query,
            long pageIndex, int pageSize, out long totalRecords,
            Func<List<TDto>, IEnumerable<TEntity>> mapDtos,
            string orderBy, Direction orderDirection, bool orderBySystemField,
            Sql<ISqlContext> filterSql = null) // fixme filter is different on v7?
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            // start with base query, and apply the supplied IQuery
            if (query == null) query = AmbientScope.SqlContext.Query<TEntity>();
            var sql = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            sql = PrepareSqlForPage(sql, filterSql, orderBy, orderDirection, orderBySystemField);

            // get a page of DTOs and the total count
            var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sql);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            // map the DTOs and return
            return mapDtos(pagedResult.Items);
        }

        protected IDictionary<int, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps)
            where T : class, IContentBase
        {
            var versions = new List<int>();
            foreach (var temp in temps)
            {
                versions.Add(temp.VersionId);
                if (temp.PublishedVersionId > 0)
                    versions.Add(temp.PublishedVersionId);
            }
            if (versions.Count == 0) return new Dictionary<int, PropertyCollection>();

            // get all PropertyDataDto for all definitions / versions
            var allPropertyDataDtos = Database.FetchByGroups<PropertyDataDto, int>(versions, 2000, batch =>
                SqlContext.Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .WhereIn<PropertyDataDto>(x => x.VersionId, batch))
                .ToList();

            // get PropertyDataDto distinct PropertyTypeDto
            var allPropertyTypeIds = allPropertyDataDtos.Select(x => x.PropertyTypeId).Distinct().ToList();
            var allPropertyTypeDtos = Database.FetchByGroups<PropertyTypeDto, int>(allPropertyTypeIds, 2000, batch =>
                SqlContext.Sql()
                    .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto))
                    .From<PropertyTypeDto>()
                    .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
                    .WhereIn<PropertyTypeDto>(x => x.Id, batch));

            // index the types for perfs, and assign to PropertyDataDto
            var indexedPropertyTypeDtos = allPropertyTypeDtos.ToDictionary(x => x.Id, x => x);
            foreach (var a in allPropertyDataDtos)
                a.PropertyTypeDto = indexedPropertyTypeDtos[a.PropertyTypeId];

            // prefetch configuration for tag properties
            var tagEditors = new Dictionary<string, TagConfiguration>();
            foreach (var propertyTypeDto in indexedPropertyTypeDtos.Values)
            {
                var editorAlias = propertyTypeDto.DataTypeDto.EditorAlias;
                var editorAttribute = PropertyEditors[editorAlias].GetTagAttribute();
                if (editorAttribute == null) continue;
                var tagConfigurationSource = propertyTypeDto.DataTypeDto.Configuration;
                var tagConfiguration = string.IsNullOrWhiteSpace(tagConfigurationSource)
                    ? new TagConfiguration()
                    : JsonConvert.DeserializeObject<TagConfiguration>(tagConfigurationSource);
                if (tagConfiguration.Delimiter == default) tagConfiguration.Delimiter = editorAttribute.Delimiter;
                tagEditors[editorAlias] = tagConfiguration;
            }

            // now we have
            // - the definitinos
            // - all property data dtos
            // - tag editors
            // and we need to build the proper property collections

            return GetPropertyCollections(temps, allPropertyDataDtos, tagEditors);
        }

        private IDictionary<int, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps, IEnumerable<PropertyDataDto> allPropertyDataDtos, Dictionary<string, TagConfiguration> tagConfigurations)
            where T : class, IContentBase
        {
            var result = new Dictionary<int, PropertyCollection>();
            var compositionPropertiesIndex = new Dictionary<int, PropertyType[]>();

            // index PropertyDataDto per versionId for perfs
            // merge edited and published dtos
            var indexedPropertyDataDtos = new Dictionary<int, List<PropertyDataDto>>();
            foreach (var dto in allPropertyDataDtos)
            {
                var versionId = dto.VersionId;
                if (indexedPropertyDataDtos.TryGetValue(versionId, out var list) == false)
                    indexedPropertyDataDtos[versionId] = list = new List<PropertyDataDto>();
                list.Add(dto);
            }

            foreach (var temp in temps)
            {
                // compositionProperties is the property types for the entire composition
                // use an index for perfs
                if (compositionPropertiesIndex.TryGetValue(temp.ContentType.Id, out var compositionProperties) == false)
                    compositionPropertiesIndex[temp.ContentType.Id] = compositionProperties = temp.ContentType.CompositionPropertyTypes.ToArray();

                // map the list of PropertyDataDto to a list of Property
                var propertyDataDtos = new List<PropertyDataDto>();
                if (indexedPropertyDataDtos.TryGetValue(temp.VersionId, out var propertyDataDtos1))
                {
                    propertyDataDtos.AddRange(propertyDataDtos1);
                    if (temp.VersionId == temp.PublishedVersionId) // dirty corner case
                        propertyDataDtos.AddRange(propertyDataDtos1.Select(x => x.Clone(-1)));
                }
                if (temp.VersionId != temp.PublishedVersionId && indexedPropertyDataDtos.TryGetValue(temp.PublishedVersionId, out var propertyDataDtos2))
                    propertyDataDtos.AddRange(propertyDataDtos2);
                var properties = PropertyFactory.BuildEntities(compositionProperties, propertyDataDtos, temp.PublishedVersionId, LanguageRepository).ToList();

                // deal with tags
                foreach (var property in properties)
                {
                    if (!tagConfigurations.TryGetValue(property.PropertyType.PropertyEditorAlias, out var tagConfiguration))
                        continue;

                    //fixme doesn't take into account variants
                    property.SetTagsValue(property.GetValue(), tagConfiguration);
                }

                if (result.ContainsKey(temp.VersionId))
                {
                    if (ContentRepositoryBase.ThrowOnWarning)
                        throw new InvalidOperationException($"The query returned multiple property sets for content {temp.Id}, {temp.ContentType.Name}");
                    Logger.Warn<ContentRepositoryBase<TId, TEntity, TRepository>>("The query returned multiple property sets for content {ContentId}, {ContentTypeName}", temp.Id, temp.ContentType.Name);
                }

                result[temp.VersionId] = new PropertyCollection(properties);
            }

            return result;
        }

        protected virtual string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            // translate the supplied "order by" field, which were originally defined for in-memory
            // object sorting of ContentItemBasic instance, to the actual database field names.

            switch (orderBy.ToUpperInvariant())
            {
                case "VERSIONDATE":
                case "UPDATEDATE":
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.ContentVersion, "versionDate");
                case "CREATEDATE":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "createDate");
                case "NAME":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "text");
                case "PUBLISHED":
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.Document, "published");
                case "OWNER":
                    //TODO: This isn't going to work very nicely because it's going to order by ID, not by letter
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "nodeUser");
                case "PATH":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "path");
                case "SORTORDER":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "sortOrder");
                default:
                    //ensure invalid SQL cannot be submitted
                    return Regex.Replace(orderBy, @"[^\w\.,`\[\]@-]", "");
            }
        }

        protected string GetDatabaseFieldNameForOrderBy(string tableName, string fieldName)
        {
            return SqlContext.SqlSyntax.GetQuotedTableName(tableName) + "." + SqlContext.SqlSyntax.GetQuotedColumnName(fieldName);
        }

        #region UnitOfWork Events

        public class ScopedEntityEventArgs : EventArgs
        {
            public ScopedEntityEventArgs(IScope scope, TEntity entity)
            {
                Scope = scope;
                Entity = entity;
            }

            public IScope Scope { get; }
            public TEntity Entity { get; }
        }

        public class ScopedVersionEventArgs : EventArgs
        {
            public ScopedVersionEventArgs(IScope scope, int entityId, int versionId)
            {
                Scope = scope;
                EntityId = entityId;
                VersionId = versionId;
            }

            public IScope Scope { get; }
            public int EntityId { get; }
            public int VersionId { get; }
        }

        public static event TypedEventHandler<TRepository, ScopedEntityEventArgs> ScopedEntityRefresh;
        public static event TypedEventHandler<TRepository, ScopedEntityEventArgs> ScopeEntityRemove;
        public static event TypedEventHandler<TRepository, ScopedVersionEventArgs> ScopeVersionRemove;

        // used by tests to clear events
        internal static void ClearScopeEvents()
        {
            ScopedEntityRefresh = null;
            ScopeEntityRemove = null;
            ScopeVersionRemove = null;
        }

        protected void OnUowRefreshedEntity(ScopedEntityEventArgs args)
        {
            ScopedEntityRefresh.RaiseEvent(args, This);
        }

        protected void OnUowRemovingEntity(ScopedEntityEventArgs args)
        {
            ScopeEntityRemove.RaiseEvent(args, This);
        }

        protected void OnUowRemovingVersion(ScopedVersionEventArgs args)
        {
            ScopeVersionRemove.RaiseEvent(args, This);
        }

        #endregion

        #region Classes

        protected class TempContent
        {
            public TempContent(int id, int versionId, int publishedVersionId, IContentTypeComposition contentType)
            {
                Id = id;
                VersionId = versionId;
                PublishedVersionId = publishedVersionId;
                ContentType = contentType;
            }

            /// <summary>
            /// Gets or sets the identifier of the content.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the version identifier of the content.
            /// </summary>
            public int VersionId { get; set; }

            /// <summary>
            /// Gets or sets the published version identifier of the content.
            /// </summary>
            public int PublishedVersionId { get; set; }

            /// <summary>
            /// Gets or sets the content type.
            /// </summary>
            public IContentTypeComposition ContentType { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the template 1 of the content.
            /// </summary>
            public int? Template1Id { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the template 2 of the content.
            /// </summary>
            public int? Template2Id { get; set; }
        }

        protected class TempContent<T> : TempContent
            where T : class, IContentBase
        {
            public TempContent(int id, int versionId, int publishedVersionId, IContentTypeComposition contentType, T content = null)
                : base(id, versionId, publishedVersionId, contentType)
            {
                Content = content;
            }

            /// <summary>
            /// Gets or sets the associated actual content.
            /// </summary>
            public T Content { get; set; }
        }

        // fixme copied from 7.6

        /// <summary>
        /// For Paging, repositories must support returning different query for the query type specified
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        protected abstract Sql<ISqlContext> GetBaseQuery(QueryType queryType);

        /*
        internal class DocumentDefinitionCollection : KeyedCollection<ValueType, DocumentDefinition>
        {
            private readonly bool _includeAllVersions;

            /// <summary>
            /// Constructor specifying if all versions should be allowed, in that case the key for the collection becomes the versionId (GUID)
            /// </summary>
            /// <param name="includeAllVersions"></param>
            public DocumentDefinitionCollection(bool includeAllVersions = false)
            {
                _includeAllVersions = includeAllVersions;
            }

            protected override ValueType GetKeyForItem(DocumentDefinition item)
            {
                return _includeAllVersions ? (ValueType)item.Version : item.Id;
            }

            /// <summary>
            /// if this key already exists if it does then we need to check
            /// if the existing item is 'older' than the new item and if that is the case we'll replace the older one
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool AddOrUpdate(DocumentDefinition item)
            {
                //if we are including all versions then just add, we aren't checking for latest
                if (_includeAllVersions)
                {
                    Add(item);
                    return true;
                }

                if (Dictionary == null)
                {
                    Add(item);
                    return true;
                }

                var key = GetKeyForItem(item);
                if (TryGetValue(key, out DocumentDefinition found))
                {
                    //it already exists and it's older so we need to replace it
                    if (item.VersionId <= found.VersionId) return false;

                    var currIndex = Items.IndexOf(found);
                    if (currIndex == -1)
                        throw new IndexOutOfRangeException("Could not find the item in the list: " + found.Version);

                    //replace the current one with the newer one
                    SetItem(currIndex, item);
                    return true;
                }

                Add(item);
                return true;
            }

            public bool TryGetValue(ValueType key, out DocumentDefinition val)
            {
                if (Dictionary != null)
                    return Dictionary.TryGetValue(key, out val);

                val = null;
                return false;
            }
        }

        /// <summary>
        /// Implements a Guid comparer that respect the Sql engine ordering.
        /// </summary>
        /// <remarks>
        /// MySql sorts Guids as strings, but MSSQL sorts guids based on a weird byte sections order
        /// This comparer compares Guids using the corresponding Sql syntax method, ie the method of the underlying Sql engine.
        /// see http://stackoverflow.com/questions/7810602/sql-server-guid-sort-algorithm-why
        /// see https://blogs.msdn.microsoft.com/sqlprogrammability/2006/11/06/how-are-guids-compared-in-sql-server-2005/
        /// </remarks>
        private class DocumentDefinitionComparer : IComparer<Guid>
        {
            private readonly bool _mySql;

            public DocumentDefinitionComparer(ISqlSyntaxProvider sqlSyntax)
            {
                _mySql = sqlSyntax is MySqlSyntaxProvider;
            }

            public int Compare(Guid x, Guid y)
            {
                // MySql sorts Guids as string (ie normal, same as .NET) whereas MSSQL
                // sorts them on a weird byte sections order
                return _mySql ? x.CompareTo(y) : new SqlGuid(x).CompareTo(new SqlGuid(y));
            }
        }

        internal class DocumentDefinition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public DocumentDefinition(DocumentDto dto, IContentTypeComposition composition)
            {
                DocumentDto = dto;
                ContentVersionDto = dto.ContentVersionDto;
                Composition = composition;
            }

            public DocumentDefinition(ContentVersionDto dto, IContentTypeComposition composition)
            {
                ContentVersionDto = dto;
                Composition = composition;
            }

            public DocumentDto DocumentDto { get; }
            public ContentVersionDto ContentVersionDto { get; }

            public int Id => ContentVersionDto.NodeId;

            public Guid Version => DocumentDto?.VersionId ?? ContentVersionDto.VersionId;

            // This is used to determien which version is the most recent
            public int VersionId => ContentVersionDto.Id;

            public DateTime VersionDate => ContentVersionDto.VersionDate;

            public DateTime CreateDate => ContentVersionDto.ContentDto.NodeDto.CreateDate;

            public IContentTypeComposition Composition { get; set; }
        }

        // Represents a query that may contain paging information.
        internal class PagingSqlQuery
        {
            // the original query sql
            public Sql QuerySql { get; }

            public PagingSqlQuery(Sql querySql)
            {
                QuerySql = querySql;
            }

            protected PagingSqlQuery(Sql querySql, int pageSize)
                : this(querySql)
            {
                HasPaging = pageSize > 0;
            }

            // whether the paging query is actually paging
            public bool HasPaging { get; }

            // the paging sql
            public virtual Sql BuildPagedQuery(string columns)
            {
                throw new InvalidOperationException("This query has no paging information.");
            }
        }

        /// <summary>
        /// Represents a query that may contain paging information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class PagingSqlQuery<T> : PagingSqlQuery // fixme what's <T> here?
        {
            private readonly Database _db;
            private readonly long _pageIndex;
            private readonly int _pageSize;

            // fixme - don't capture a db instance here!
            // instead, should have an extension method, so one can do
            // sql = db.BuildPageQuery(pagingQuery, columns)
            public PagingSqlQuery(Database db, Sql querySql, long pageIndex, int pageSize)
                : base(querySql, pageSize)
            {
                _db = db;
                _pageIndex = pageIndex;
                _pageSize = pageSize;
            }

            /// <summary>
            /// Creates a paged query based on the original query and subtitutes the selectColumns specified
            /// </summary>
            /// <param name="columns"></param>
            /// <returns></returns>
            // build a page query
            public override Sql BuildPagedQuery(string columns)
            {
                if (HasPaging == false)
                    throw new InvalidOperationException("This query has no paging information.");

                // substitutes the original "SELECT ..." with "SELECT {columns}" ie only
                // select the specified columns - fixme why?
                var sql = $"SELECT {columns} {QuerySql.SQL.Substring(QuerySql.SQL.IndexOf("FROM", StringComparison.Ordinal))}";

                // and then build the page query
                var args = QuerySql.Arguments;
                _db.BuildPageQueries<T>(_pageIndex * _pageSize, _pageSize, sql, ref args, out string unused, out string sqlPage);
                return new Sql(sqlPage, args);
            }
        }
        */

        #endregion

        #region Utilities

        protected virtual string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.EnsureUniqueNodeName", tsql => tsql
                .Select<NodeDto>(x => Alias(x.NodeId, "id"), x => Alias(x.Text, "name"))
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") && x.ParentId == SqlTemplate.Arg<int>("parentId")));

            var sql = template.Sql(NodeObjectTypeId, parentId);
            var names = Database.Fetch<SimilarNodeName>(sql);

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        protected virtual int GetNewChildSortOrder(int parentId, int first)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetSortOrder", tsql =>
                tsql.Select($"COALESCE(MAX(sortOrder),{first - 1})").From<NodeDto>().Where<NodeDto>(x => x.ParentId == SqlTemplate.Arg<int>("parentId") && x.NodeObjectType == NodeObjectTypeId)
            );

            return Database.ExecuteScalar<int>(template.Sql(new { parentId })) + 1;
        }

        protected virtual NodeDto GetParentNodeDto(int parentId)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetParentNode", tsql =>
                tsql.Select<NodeDto>().From<NodeDto>().Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId"))
            );

            return Database.Fetch<NodeDto>(template.Sql(parentId)).First();
        }

        protected virtual int GetReservedId(Guid uniqueId)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetReservedId", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).From<NodeDto>().Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("uniqueId") && x.NodeObjectType == Constants.ObjectTypes.IdReservation)
            );
            var id = Database.ExecuteScalar<int?>(template.Sql(new { uniqueId = uniqueId }));
            return id ?? 0;
        }

        #endregion

        #region Recycle bin

        public abstract int RecycleBinId { get; }

        public virtual IEnumerable<TEntity> GetRecycleBin()
        {
            return Get(Query<TEntity>().Where(entity => entity.Trashed));
        }

        #endregion
    }
}
