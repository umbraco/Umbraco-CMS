using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.DI;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    internal sealed class VersionableRepositoryBase
    {
        /// <summary>
        /// This is used for unit tests ONLY
        /// </summary>
        public static bool ThrowOnWarning = false;
    }

    internal abstract class VersionableRepositoryBase<TId, TEntity, TRepository> : NPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
        where TRepository : class, IRepository
    {
        //private readonly IContentSection _contentSection;

        protected VersionableRepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger /*, IContentSection contentSection*/)
            : base(work, cache, logger)
        {
            //_contentSection = contentSection;
        }

        protected abstract TRepository This { get; }

        #region IRepositoryVersionable Implementation

        /// <summary>
        /// Gets a list of all versions for an <see cref="TEntity"/> ordered so latest is first
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> to retrieve versions from</param>
        /// <returns>An enumerable list of the same <see cref="TEntity"/> object with different versions</returns>
        public virtual IEnumerable<TEntity> GetAllVersions(int id)
        {
            var sql = Sql()
                .SelectAll()
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<NodeDto>(x => x.NodeId == id)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos = Database.Fetch<ContentVersionDto>(sql);
            return dtos.Select(x => GetByVersion(x.VersionId));
        }

        /// <summary>
        /// Gets a list of all version Ids for the given content item ordered so latest is first
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maxRows">The maximum number of rows to return</param>
        /// <returns></returns>
        public virtual IEnumerable<Guid> GetVersionIds(int id, int maxRows)
        {
            var sql = Sql();
            sql.Select("cmsDocument.versionId")
                .From<DocumentDto>()
                .InnerJoin<ContentDto>()
                .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<NodeDto>(x => x.NodeId == id)
                .OrderByDescending<DocumentDto>(x => x.UpdateDate);

            return Database.Fetch<Guid>(SqlSyntax.SelectTop(sql, maxRows));
        }

        public virtual void DeleteVersion(Guid versionId)
        {
            var dto = Database.FirstOrDefault<ContentVersionDto>("WHERE versionId = @VersionId", new { VersionId = versionId });
            if(dto == null) return;

            //Ensure that the lastest version is not deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = dto.NodeId });
            if(latestVersionDto.VersionId == dto.VersionId)
                return;

            PerformDeleteVersion(dto.NodeId, versionId);
        }

        public virtual void DeleteVersions(int id, DateTime versionDate)
        {
            //Ensure that the latest version is not part of the versions being deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = id });
            var list =
                Database.Fetch<ContentVersionDto>(
                    "WHERE versionId <> @VersionId AND (ContentId = @Id AND VersionDate < @VersionDate)",
                    new { /*VersionId =*/ latestVersionDto.VersionId, Id = id, VersionDate = versionDate});
            if (list.Any() == false) return;

            foreach (var dto in list)
            {
                PerformDeleteVersion(id, dto.VersionId);
            }
        }

        public abstract TEntity GetByVersion(Guid versionId);

        /// <summary>
        /// Protected method to execute the delete statements for removing a single version for a TEntity item.
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> to delete a version from</param>
        /// <param name="versionId">Guid id of the version to delete</param>
        protected abstract void PerformDeleteVersion(int id, Guid versionId);

        #endregion

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            var pathMatch = parentId == -1
                ? "-1,"
                : "," + parentId + ",";

            var sql = Sql()
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

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            var sql = Sql()
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
        /// Get the total count of entities
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        public int Count(string contentTypeAlias = null)
        {
            var sql = Sql()
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

        /// <summary>
        /// This removes associated tags from the entity - used generally when an entity is recycled
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tagRepo"></param>
        protected void ClearEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            tagRepo.ClearTagsFromEntity(entity.Id);
        }

        /// <summary>
        /// Updates the tag repository with any tag enabled properties and their values
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tagRepo"></param>
        protected void UpdateEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            foreach (var tagProp in entity.Properties.Where(x => x.TagSupport.Enable))
            {
                if (tagProp.TagSupport.Behavior == PropertyTagBehavior.Remove)
                {
                    //remove the specific tags
                    tagRepo.RemoveTagsFromProperty(
                        entity.Id,
                        tagProp.PropertyTypeId,
                        tagProp.TagSupport.Tags.Select(x => new Tag { Text = x.Item1, Group = x.Item2 }));
                }
                else
                {
                    //assign the tags
                    tagRepo.AssignTagsToProperty(
                        entity.Id,
                        tagProp.PropertyTypeId,
                        tagProp.TagSupport.Tags.Select(x => new Tag { Text = x.Item1, Group = x.Item2 }),
                        tagProp.TagSupport.Behavior == PropertyTagBehavior.Replace);
                }
            }
        }

        protected bool HasTagProperty(IContentBase entity)
        {
            return entity.Properties.Any(x => x.TagSupport.Enable);
        }

        private Sql<SqlContext> PrepareSqlForPagedResults(Sql<SqlContext> sql, Sql<SqlContext> filterSql, string orderBy, Direction orderDirection, bool orderBySystemField, string table)
        {
            if (filterSql == null && string.IsNullOrEmpty(orderBy)) return sql;

            // preserve original
            var psql = new Sql<SqlContext>(sql.SqlContext, sql.SQL, sql.Arguments);

            // apply filter
            if (filterSql != null)
                psql.Append(filterSql);

            // non-sorting, we're done
            if (string.IsNullOrEmpty(orderBy))
                return psql;

            // else apply sort
            var dbfield = orderBySystemField
                ? GetOrderBySystemField(ref psql, orderBy)
                : GetOrderByNonSystemField(ref psql, orderBy, table);

            if (orderDirection == Direction.Ascending)
                psql.OrderBy(dbfield);
            else
                psql.OrderByDescending(dbfield);

            // no matter what we always MUST order the result also by umbracoNode.id to ensure that all records being ordered by are unique.
            // if we do not do this then we end up with issues where we are ordering by a field that has duplicate values (i.e. the 'text' column
            // is empty for many nodes)
            // see: http://issues.umbraco.org/issue/U4-8831
            // fixme - commented out is 7.6 and looks suspicious ??!!
            //if (orderBySystemField && orderBy.InvariantEquals("umbraconode.id") == false)
            dbfield = GetDatabaseFieldNameForOrderBy("umbracoNode", "id");
            if (orderBySystemField == false || orderBy.InvariantEquals(dbfield) == false)
            {
                var matches = VersionableRepositoryBaseAliasRegex.For(SqlSyntax).Matches(sql.SQL);
                var match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(dbfield));
                if (match != null)
                    dbfield = match.Groups[2].Value;
                psql.OrderBy(dbfield);
            }

            // fixme - temp - for the time being NPoco PagingHelper cannot deal with multiline
            psql = new Sql<SqlContext>(psql.SqlContext, psql.SQL.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "), psql.Arguments);

            return psql;
        }

        private string GetOrderBySystemField(ref Sql<SqlContext> sql, string orderBy)
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

            var matches = VersionableRepositoryBaseAliasRegex.For(SqlSyntax).Matches(sql.SQL);
            var match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(dbfield));
            if (match != null)
                dbfield = match.Groups[2].Value;

            return dbfield;
        }

        private string GetOrderByNonSystemField(ref Sql<SqlContext> sql, string orderBy, string table)
        {
            // Sorting by a custom field, so set-up sub-query for ORDER BY clause to pull through value
            // from most recent content version for the given order by field
            var sortedInt = string.Format(SqlSyntax.ConvertIntegerToOrderableString, "dataInt");
            var sortedDate = string.Format(SqlSyntax.ConvertDateToOrderableString, "dataDate");
            var sortedString = "COALESCE(dataNvarchar,'')"; // assuming COALESCE is ok for all syntaxes
            var sortedDecimal = string.Format(SqlSyntax.ConvertDecimalToOrderableString, "dataDecimal");

            // variable query fragments that depend on what we are querying
            string andVersion, andNewest, idField;
            switch (table)
            {
                case "cmsDocument":
                    andVersion = " AND cpd.versionId = cd.versionId";
                    andNewest = " AND cd.newest = 1";
                    idField = "nodeId";
                    break;
                case "cmsMember":
                    andVersion = string.Empty;
                    andNewest = string.Empty;
                    idField = "nodeId";
                    break;
                case "cmsContentVersion":
                    andVersion = " AND cpd.versionId = cd.versionId";
                    andNewest = string.Empty;
                    idField = "contentId";
                    break;
                default:
                    throw new NotSupportedException($"Table {table} is not supported.");
            }

            // needs to be an outer join since there's no guarantee that any of the nodes have values for this property
            var outerJoinTempTable = $@"LEFT OUTER JOIN (
                    SELECT CASE
                        WHEN dataInt IS NOT NULL THEN {sortedInt}
                        WHEN dataDecimal IS NOT NULL THEN {sortedDecimal}
                        WHEN dataDate IS NOT NULL THEN {sortedDate}
                        ELSE {sortedString}
                    END AS CustomPropVal,
                    cd.{idField} AS CustomPropValContentId
                    FROM {table} cd
                    INNER JOIN cmsPropertyData cpd ON cpd.contentNodeId = cd.{idField}{andVersion}
                    INNER JOIN cmsPropertyType cpt ON cpt.Id = cpd.propertytypeId
                    WHERE cpt.Alias = @{sql.Arguments.Length}{andNewest}) AS CustomPropData
                    ON CustomPropData.CustomPropValContentId = umbracoNode.id "; // trailing space is important!

            // insert this just above the last WHERE
            var pos = sql.SQL.InvariantIndexOf("WHERE");
            if (pos < 0) throw new Exception("Oops, WHERE not found.");
            var newSql = sql.SQL.Insert(pos, outerJoinTempTable);

            var newArgs = sql.Arguments.ToList();
            newArgs.Add(orderBy);

            // insert the SQL selected field, too, else ordering cannot work
            if (sql.SQL.StartsWith("SELECT ") == false) throw new Exception("Oops, SELECT not found.");
            newSql = newSql.Insert("SELECT ".Length, "CustomPropData.CustomPropVal, ");

            sql = new Sql<SqlContext>(sql.SqlContext, newSql, newArgs.ToArray());

            // and order by the custom field
            return "CustomPropData.CustomPropVal";
        }

        // fixme
        //
        // mergin from 7.6... well we cannot just merge what comes from 7.6 - too much distance
        //
        // need to understand what's been done in 7.6 and why, and reproduce here
        // - 2a4e73c on 01/22 uses static factories, well we should use them everywhere but l8tr
        // - c7b505f on 01/24 introduces QueryType and 2 queries for fetching content <<< why?
        //                      because in 7.6, ProcessQuery calls GetPropertyCollection passing the sql which runs again
        //                      but in v8 GetPropertyCollection has been refactored to use WhereIn() instead of a subquery
        //                      as in most cases we won't get more than 2000 items -> nothing to do
        // - 7f905bc on 01/26 fixes a "nasty" issue with reading properties
        // - f192f24 on 01/31 fixes paged queries getting *all* property data
        // - 86b2dac on 01/31 introduces the whole PagingQuery thing
        // - 32d757b on 01/31 introduces the QueryType.Single vs .Many difference
        // - af287c3 on 02/16 improves corruption handling for content (published/newest)
        // - (more) on 02/22-27 fixes various minor issues
        //
        // so we want
        // - to deal with the number of queries and getting *all* property data and outer joins
        // - to deal with published/newest corruption
        //
        // but getting property data was already optimized, and we prob don't need the two queries thing
        // so... doing our best but not really merging v7
        // see also Content/Member/Media repositories

        protected IEnumerable<TEntity> GetPagedResultsByQuery<TDto>(IQuery<TEntity> query, long pageIndex, int pageSize, out long totalRecords,
            Func<List<TDto>, IEnumerable<TEntity>> mapper,
            string orderBy, Direction orderDirection, bool orderBySystemField, string table,
            Sql<SqlContext> filterSql = null)
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            // start with base query, and apply the supplied IQuery
            if (query == null) query = QueryT;
            var sqlNodeIds = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            sqlNodeIds = PrepareSqlForPagedResults(sqlNodeIds, filterSql, orderBy, orderDirection, orderBySystemField, table);

            // get a page of DTOs and the total count
            var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sqlNodeIds);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            // map the DTOs and return
            return mapper(pagedResult.Items);
        }

        protected IDictionary<Guid, PropertyCollection> GetPropertyCollection(List<TempContent> temps)
        {
            var versions = temps.Select(x => x.Version).ToArray();
            if (versions.Length == 0) return new Dictionary<Guid, PropertyCollection>();

            // get all PropertyDataDto for all definitions / versions
            var allPropertyDataDtos = Database.FetchByGroups<PropertyDataDto, Guid>(versions, 2000, batch =>
                Sql()
                    .Select<PropertyDataDto>()
                    .WhereIn<PropertyDataDto>(x => x.VersionId, batch))
                .ToList();

            // get PropertyDataDto distinct PropertyTypeDto
            var allPropertyTypeIds = allPropertyDataDtos.Select(x => x.PropertyTypeId).Distinct().ToList();
            var allPropertyTypeDtos = Database.FetchByGroups<PropertyTypeDto, int>(allPropertyTypeIds, 2000, batch => 
                Sql()
                    .Select<PropertyTypeDto>()
                    .WhereIn<PropertyTypeDto>(x => x.Id, batch));

            // index the types for perfs, and assign to PropertyDataDto
            var indexedPropertyTypeDtos = allPropertyTypeDtos.ToDictionary(x => x.Id, x => x);
            foreach (var a in allPropertyDataDtos)
                a.PropertyTypeDto = indexedPropertyTypeDtos[a.PropertyTypeId];

            // lazy access to prevalue for data types if any property requires tag support
            var pre = new Lazy<IEnumerable<DataTypePreValueDto>>(() =>
            {
                return Database.FetchByGroups<DataTypePreValueDto, int>(allPropertyTypeIds, 2000, batch =>
                    Sql()
                        .Select<DataTypePreValueDto>()
                        .From<DataTypePreValueDto>()
                        .WhereIn<DataTypePreValueDto>(x => x.DataTypeNodeId, batch));
            });

            // now we have
            // - the definitinos
            // - all property data dtos
            // - a lazy access to prevalues
            // and we need to build the proper property collections

            return GetPropertyCollection(temps, allPropertyDataDtos, pre);
        }

        private IDictionary<Guid, PropertyCollection> GetPropertyCollection(List<TempContent> temps, IEnumerable<PropertyDataDto> allPropertyDataDtos, Lazy<IEnumerable<DataTypePreValueDto>> allPreValues)
        {
            var result = new Dictionary<Guid, PropertyCollection>();
            var propertiesWithTagSupport = new Dictionary<string, SupportTagsAttribute>();
            var compositionPropertiesIndex = new Dictionary<int, PropertyType[]>();

            // index PropertyDataDto per versionId for perfs
            var indexedPropertyDataDtos = new Dictionary<Guid, List<PropertyDataDto>>();
            foreach (var dto in allPropertyDataDtos)
            {
                var version = dto.VersionId.Value;
                if (indexedPropertyDataDtos.TryGetValue(version, out var list) == false)
                    indexedPropertyDataDtos[version] = list = new List<PropertyDataDto>();
                list.Add(dto);
            }

            foreach (var temp in temps)
            {
                // compositionProperties is the property types for the entire composition
                // use an index for perfs
                if (compositionPropertiesIndex.TryGetValue(temp.Composition.Id, out PropertyType[] compositionProperties) == false)
                    compositionPropertiesIndex[temp.Composition.Id] = compositionProperties = temp.Composition.CompositionPropertyTypes.ToArray();

                // map the list of PropertyDataDto to a list of Property
                var properties = indexedPropertyDataDtos.TryGetValue(temp.Version, out var propertyDataDtos)
                    ? PropertyFactory.BuildEntity(propertyDataDtos, compositionProperties, temp.CreateDate, temp.VersionDate).ToList()
                    : new List<Property>();

                // deal with tags
                Dictionary<string, object> additionalData = null;
                foreach (var property in properties)
                {
                    // test for support and cache
                    var editor = Current.PropertyEditors[property.PropertyType.PropertyEditorAlias];
                    if (propertiesWithTagSupport.TryGetValue(property.PropertyType.PropertyEditorAlias, out SupportTagsAttribute tagSupport) == false)
                        propertiesWithTagSupport[property.PropertyType.PropertyEditorAlias] = tagSupport = TagExtractor.GetAttribute(editor);
                    if (tagSupport == null) continue;

                    //this property has tags, so we need to extract them and for that we need the prevals which we've already looked up
                    // fixme - optimize with index
                    var preValData = allPreValues.Value.Where(x => x.DataTypeNodeId == property.PropertyType.DataTypeDefinitionId)
                        .Distinct()
                        .ToArray();

                    // build and set tags
                    var asDictionary = preValData.ToDictionary(x => x.Alias, x => new PreValue(x.Id, x.Value, x.SortOrder));
                    var preVals = new PreValueCollection(asDictionary);
                    if (additionalData == null) additionalData = new Dictionary<string, object>(); // reduce allocs
                    var contentPropData = new ContentPropertyData(property.Value, preVals, additionalData);
                    TagExtractor.SetPropertyTags(property, contentPropData, property.Value, tagSupport);
                }

                if (result.ContainsKey(temp.Version))
                {
                    var msg = $"The query returned multiple property sets for content {temp.Id}, {temp.Composition.Name}";
                    if (VersionableRepositoryBase.ThrowOnWarning)
                        throw new InvalidOperationException(msg);
                    Logger.Warn<VersionableRepositoryBase<TId, TEntity, TRepository>>(msg);
                }

                result[temp.Version] = new PropertyCollection(properties);
            }

            //// iterate each definition grouped by it's content type,
            //// this will mean less property type iterations while building
            //// up the property collections
            //foreach (var compositionGroup in documentDefs.GroupBy(x => x.Composition))
            //{
            //    // compositionGroup.Key is the composition
            //    // compositionProperties is the property types for the entire composition
            //    var compositionProperties = compositionGroup.Key.CompositionPropertyTypes.ToArray();

            //    foreach (var def in compositionGroup)
            //    {
            //        var properties = indexedPropertyDataDtos.TryGetValue(def.Version, out var propertyDataDtos)
            //            ? PropertyFactory.BuildEntity(propertyDataDtos, compositionProperties, def.CreateDate, def.VersionDate).ToList()
            //            : new List<Property>();

            //        foreach (var property in properties)
            //        {
            //            //NOTE: The benchmarks run with and without the following code show very little change so this is not a perf bottleneck
            //            var editor = Current.PropertyEditors[property.PropertyType.PropertyEditorAlias];

            //            var tagSupport = propertiesWithTagSupport.ContainsKey(property.PropertyType.PropertyEditorAlias)
            //                ? propertiesWithTagSupport[property.PropertyType.PropertyEditorAlias]
            //                : TagExtractor.GetAttribute(editor);

            //            if (tagSupport == null) continue;

            //            //add to local cache so we don't need to reflect next time for this property editor alias
            //            propertiesWithTagSupport[property.PropertyType.PropertyEditorAlias] = tagSupport;

            //            //this property has tags, so we need to extract them and for that we need the prevals which we've already looked up
            //            var preValData = allPreValues.Value.Where(x => x.DataTypeNodeId == property.PropertyType.DataTypeDefinitionId)
            //                .Distinct()
            //                .ToArray();

            //            var asDictionary = preValData.ToDictionary(x => x.Alias, x => new PreValue(x.Id, x.Value, x.SortOrder));
            //            var preVals = new PreValueCollection(asDictionary);

            //            var contentPropData = new ContentPropertyData(property.Value, preVals, new Dictionary<string, object>());

            //            TagExtractor.SetPropertyTags(property, contentPropData, property.Value, tagSupport);
            //        }

            //        if (result.ContainsKey(def.Version))
            //        {
            //            var msg = $"The query returned multiple property sets for document definition {def.Id}, {def.Composition.Name}";
            //            if (ThrowOnWarning)
            //                throw new InvalidOperationException(msg);
            //            Logger.Warn<VersionableRepositoryBase<TId, TEntity, TRepository>>(msg);
            //        }

            //        result[def.Version] = new PropertyCollection(properties);
            //    }
            //}

            return result;
        }

        // fixme - copied from 7.6...
        /*
        /// <summary>
        /// A helper method for inheritors to get the paged results by query in a way that minimizes queries
        /// </summary>
        /// <typeparam name="TDto">The type of the d.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="nodeIdSelect">The tablename + column name for the SELECT statement fragment to return the node id from the query</param>
        /// <param name="defaultFilter">A callback to create the default filter to be applied if there is one</param>
        /// <param name="processQuery">A callback to process the query result</param>
        /// <param name="orderBy">The order by column</param>
        /// <param name="orderDirection">The order direction.</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">orderBy</exception>
        protected IEnumerable<TEntity> GetPagedResultsByQuery<TDto>(IQuery<TEntity> query, long pageIndex, int pageSize, out long totalRecords,
            Tuple<string, string> nodeIdSelect,
            Func<Sql, PagingSqlQuery<TDto>, IEnumerable<TEntity>> processQuery,
            string orderBy,
            Direction orderDirection,
            bool orderBySystemField,
            Func<Tuple<string, object[]>> defaultFilter = null)
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            if (query == null) query = QueryT;
            var sqlIds = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Ids), query).Translate();
            var sqlMany = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            var prepSqlIds = PrepareSqlForPagedResults(sqlIds, filterSql, orderBy, orderDirection, orderBySystemField, table);
            //var sqlNodeIdsWithSort = GetSortedSqlForPagedResults(
            //    GetFilteredSqlForPagedResults(sqlQueryIdsOnly, defaultFilter),
            //    orderDirection, orderBy, orderBySystemField, nodeIdSelect);

            // get a page of DTOs and the total count
            var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sqlIds);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);
            //var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sqlNodeIdsWithSort);
            //totalRecords = Convert.ToInt32(pagedResult.TotalItems);


            // need to check the actual items returned, not the 'totalRecords', that is because if you request a page number
            // that doesn't actually have any data on it, the totalRecords will still indicate there are records but there are none in
            // the pageResult, then the GetAll will actually return ALL records in the db.
            if (pagedResult.Items.Any() == false)
                return Enumerable.Empty<TEntity>();

            //Create the inner paged query that was used above to get the paged result, we'll use that as the inner sub query
            var args = prepSqlIds.Arguments;
            Database.BuildPageQueries<TDto>(pageIndex * pageSize, pageSize, prepSqlIds.SQL, ref args, out string unused, out string sqlPage);

            //We need to make this FULL query an inner join on the paged ID query
            var splitQuery = sqlMany.SQL.Split(new[] { "WHERE " }, StringSplitOptions.None);
            var fullQueryWithPagedInnerJoin = new Sql(splitQuery[0])
                .Append("INNER JOIN (")
                //join the paged query with the paged query arguments
                .Append(sqlPage, args)
                .Append(") temp ")
                .Append(string.Format("ON {0}.{1} = temp.{1}", nodeIdSelect.Item1, nodeIdSelect.Item2))
                //add the original where clause back with the original arguments
                .Where(splitQuery[1], sqlIds.Arguments);

            //get sorted and filtered sql
            var fullQuery = GetSortedSqlForPagedResults(
                GetFilteredSqlForPagedResults(fullQueryWithPagedInnerJoin, defaultFilter),
                orderDirection, orderBy, orderBySystemField, nodeIdSelect);

            return processQuery(fullQuery, new PagingSqlQuery<TDto>(Database, sqlNodeIdsWithSort, pageIndex, pageSize));
        }

        protected IDictionary<Guid, PropertyCollection> GetPropertyCollection(Sql sql, IReadOnlyCollection<DocumentDefinition> documentDefs)
        {
            return GetPropertyCollection(new PagingSqlQuery(sql), documentDefs);
        }

        protected IDictionary<Guid, PropertyCollection> GetPropertyCollection(PagingSqlQuery pagingSqlQuery, IReadOnlyCollection<DocumentDefinition> documentDefs)
        {
            if (documentDefs.Count == 0) return new Dictionary<Guid, PropertyCollection>();

            //initialize to the query passed in
            var docSql = pagingSqlQuery.QuerySql;

            //we need to parse the original SQL statement and reduce the columns to just cmsContent.nodeId, cmsContentVersion.VersionId so that we can use
            // the statement to go get the property data for all of the items by using an inner join
            var parsedOriginalSql = "SELECT {0} " + docSql.SQL.Substring(docSql.SQL.IndexOf("FROM", StringComparison.Ordinal));

            if (pagingSqlQuery.HasPaging)
            {
                //if this is a paged query, build the paged query with the custom column substitution, then re-assign
                docSql = pagingSqlQuery.BuildPagedQuery("{0}");
                parsedOriginalSql = docSql.SQL;
            }
            else if (parsedOriginalSql.InvariantContains("ORDER BY "))
            {
                //now remove everything from an Orderby clause and beyond if this is unpaged data
                parsedOriginalSql = parsedOriginalSql.Substring(0, parsedOriginalSql.LastIndexOf("ORDER BY ", StringComparison.Ordinal));
            }

            //This retrieves all pre-values for all data types that are referenced for all property types
            // that exist in the data set.
            //Benchmarks show that eagerly loading these so that we can lazily read the property data
            // below (with the use of Query intead of Fetch) go about 30% faster, so we'll eagerly load
            // this now since we cannot execute another reader inside of reading the property data.
            var preValsSql = new Sql(@"SELECT a.id, a.value, a.sortorder, a.alias, a.datatypeNodeId
FROM cmsDataTypePreValues a
WHERE EXISTS(
    SELECT DISTINCT b.id as preValIdInner
    FROM cmsDataTypePreValues b
	INNER JOIN cmsPropertyType
	ON b.datatypeNodeId = cmsPropertyType.dataTypeId
    INNER JOIN 
	    (" + string.Format(parsedOriginalSql, "cmsContent.contentType") + @") as docData
    ON cmsPropertyType.contentTypeId = docData.contentType
    WHERE a.id = b.id)", docSql.Arguments);

            var allPreValues = Database.Fetch<DataTypePreValueDto>(preValsSql);

            //It's Important with the sort order here! We require this to be sorted by node id,
            // this is required because this data set can be huge depending on the page size. Due
            // to it's size we need to be smart about iterating over the property values to build
            // the document. Before we used to use Linq to get the property data for a given content node
            // and perform a Distinct() call. This kills performance because that would mean if we had 7000 nodes
            // and on each iteration we will perform a lookup on potentially 100,000 property rows against the node
            // id which turns out to be a crazy amount of iterations. Instead we know it's sorted by this value we'll
            // keep an index stored of the rows being read so we never have to re-iterate the entire data set
            // on each document iteration.
            var propSql = new Sql(@"SELECT cmsPropertyData.*
FROM cmsPropertyData
INNER JOIN cmsPropertyType
ON cmsPropertyData.propertytypeid = cmsPropertyType.id
INNER JOIN 
	(" + string.Format(parsedOriginalSql, "cmsContent.nodeId, cmsContentVersion.VersionId") + @") as docData
ON cmsPropertyData.versionId = docData.VersionId AND cmsPropertyData.contentNodeId = docData.nodeId
ORDER BY contentNodeId, versionId, propertytypeid
", docSql.Arguments);

            //This does NOT fetch all data into memory in a list, this will read
            // over the records as a data reader, this is much better for performance and memory,
            // but it means that during the reading of this data set, nothing else can be read
            // from SQL server otherwise we'll get an exception.
            var allPropertyData = Database.Query<PropertyDataDto>(propSql);

            var result = new Dictionary<Guid, PropertyCollection>();
            var propertiesWithTagSupport = new Dictionary<string, SupportTagsAttribute>();
            //used to track the resolved composition property types per content type so we don't have to re-resolve (ToArray) the list every time
            var resolvedCompositionProperties = new Dictionary<int, PropertyType[]>();

            //keep track of the current property data item being enumerated
            var propertyDataSetEnumerator = allPropertyData.GetEnumerator();
            var hasCurrent = false; // initially there is no enumerator.Current

            var comparer = new DocumentDefinitionComparer(SqlSyntax);

            try
            {
                //This must be sorted by node id because this is how we are sorting the query to lookup property types above,
                // which allows us to more efficiently iterate over the large data set of property values
                foreach (var def in documentDefs.OrderBy(x => x.Id).ThenBy(x => x.Version, comparer))
                {
                    // get the resolved properties from our local cache, or resolve them and put them in cache
                    PropertyType[] compositionProperties;
                    if (resolvedCompositionProperties.ContainsKey(def.Composition.Id))
                    {
                        compositionProperties = resolvedCompositionProperties[def.Composition.Id];
                    }
                    else
                    {
                        compositionProperties = def.Composition.CompositionPropertyTypes.ToArray();
                        resolvedCompositionProperties[def.Composition.Id] = compositionProperties;
                    }

                    // assemble the dtos for this def
                    // use the available enumerator.Current if any else move to next
                    var propertyDataDtos = new List<PropertyDataDto>();
                    while (hasCurrent || propertyDataSetEnumerator.MoveNext())
                    {
                        //Not checking null on VersionId because it can never be null - no idea why it's set to nullable
                        // ReSharper disable once PossibleInvalidOperationException
                        if (propertyDataSetEnumerator.Current.VersionId.Value == def.Version)
                        {
                            hasCurrent = false; // enumerator.Current is not available
                            propertyDataDtos.Add(propertyDataSetEnumerator.Current);
                        }
                        else
                        {
                            hasCurrent = true;  // enumerator.Current is available for another def
                            break;              // no more propertyDataDto for this def
                        }
                    }

                    var properties = PropertyFactory.BuildEntity(propertyDataDtos, compositionProperties, def.CreateDate, def.VersionDate).ToArray();

                    foreach (var property in properties)
                    {
                        //NOTE: The benchmarks run with and without the following code show very little change so this is not a perf bottleneck
                        var editor = Current.PropertyEditors[property.PropertyType.PropertyEditorAlias];

                        var tagSupport = propertiesWithTagSupport.ContainsKey(property.PropertyType.PropertyEditorAlias)
                            ? propertiesWithTagSupport[property.PropertyType.PropertyEditorAlias]
                            : TagExtractor.GetAttribute(editor);

                        if (tagSupport != null)
                        {
                            //add to local cache so we don't need to reflect next time for this property editor alias
                            propertiesWithTagSupport[property.PropertyType.PropertyEditorAlias] = tagSupport;

                            //this property has tags, so we need to extract them and for that we need the prevals which we've already looked up
                            var preValData = allPreValues.Where(x => x.DataTypeNodeId == property.PropertyType.DataTypeDefinitionId)
                                .Distinct()
                                .ToArray();

                            var asDictionary = preValData.ToDictionary(x => x.Alias, x => new PreValue(x.Id, x.Value, x.SortOrder));

                            var preVals = new PreValueCollection(asDictionary);

                            var contentPropData = new ContentPropertyData(property.Value, preVals, new Dictionary<string, object>());

                            TagExtractor.SetPropertyTags(property, contentPropData, property.Value, tagSupport);
                        }
                    }

                    if (result.ContainsKey(def.Version))
                    {
                        var msg = string.Format("The query returned multiple property sets for document definition {0}, {1}, {2}", def.Id, def.Version, def.Composition.Name);
                        if (ThrowOnWarning)
                        {
                            throw new InvalidOperationException(msg);
                        }
                        else
                        {
                            Logger.Warn<VersionableRepositoryBase<TId, TEntity, TRepository>>(msg);
                        }
                    }
                    result[def.Version] = new PropertyCollection(properties);
                }
            }
            finally
            {
                propertyDataSetEnumerator.Dispose();
            }

            return result;

        }
        */

        protected virtual string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            // translate the supplied "order by" field, which were originally defined for in-memory
            // object sorting of ContentItemBasic instance, to the actual database field names.

            switch (orderBy.ToUpperInvariant())
            {
                case "VERSIONDATE":
                case "UPDATEDATE":
                    return GetDatabaseFieldNameForOrderBy("cmsContentVersion", "versionDate");
                case "CREATEDATE":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "createDate");
                case "NAME":
                    return GetDatabaseFieldNameForOrderBy("umbracoNode", "text");
                case "PUBLISHED":
                    return GetDatabaseFieldNameForOrderBy("cmsDocument", "published");
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
            return SqlSyntax.GetQuotedTableName(tableName) + "." + SqlSyntax.GetQuotedColumnName(fieldName);
        }

        #region UnitOfWork Events

        public class UnitOfWorkEntityEventArgs : EventArgs
        {
            public UnitOfWorkEntityEventArgs(IDatabaseUnitOfWork unitOfWork, TEntity entity)
            {
                UnitOfWork = unitOfWork;
                Entity = entity;
            }

            public IDatabaseUnitOfWork UnitOfWork { get; }
            public TEntity Entity { get; }
        }

        public class UnitOfWorkVersionEventArgs : EventArgs
        {
            public UnitOfWorkVersionEventArgs(IDatabaseUnitOfWork unitOfWork, int entityId, Guid versionId)
            {
                UnitOfWork = unitOfWork;
                EntityId = entityId;
                VersionId = versionId;
            }

            public IDatabaseUnitOfWork UnitOfWork { get; }
            public int EntityId { get; }
            public Guid VersionId { get; }
        }

        public static event TypedEventHandler<TRepository, UnitOfWorkEntityEventArgs> UowRefreshedEntity;
        public static event TypedEventHandler<TRepository, UnitOfWorkEntityEventArgs> UowRemovingEntity;
        public static event TypedEventHandler<TRepository, UnitOfWorkVersionEventArgs> UowRemovingVersion;

        protected void OnUowRefreshedEntity(UnitOfWorkEntityEventArgs args)
        {
            UowRefreshedEntity.RaiseEvent(args, This);
        }

        protected void OnUowRemovingEntity(UnitOfWorkEntityEventArgs args)
        {
            UowRemovingEntity.RaiseEvent(args, This);
        }

        protected void OnUowRemovingVersion(UnitOfWorkVersionEventArgs args)
        {
            UowRemovingVersion.RaiseEvent(args, This);
        }

        #endregion

        #region Classes

        protected class TempContent
        {
            public TempContent(int id, Guid version, DateTime versionDate, DateTime createDate, IContentTypeComposition composition, IContentBase content = null)
            {
                Id = id;
                Version = version;
                VersionDate = versionDate;
                CreateDate = createDate;
                Composition = composition;
                Content = content;
            }

            public int Id { get; set; }
            public Guid Version { get; set; }
            public DateTime VersionDate { get; set; }
            public DateTime CreateDate { get; set; }
            public IContentTypeComposition Composition { get; set; }

            public IContentBase Content { get; set; }

            public int? TemplateId { get; set; }
        }

        // fixme copied from 7.6

        /// <summary>
        /// For Paging, repositories must support returning different query for the query type specified
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        protected abstract Sql<SqlContext> GetBaseQuery(QueryType queryType);

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
    }
}