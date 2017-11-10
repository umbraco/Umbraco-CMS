using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
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
        protected VersionableRepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
            : base(work, cache, logger)
        { }

        protected abstract TRepository This { get; }

        #region Versions

        // gets a specific version
        public abstract TEntity GetByVersion(Guid versionId);

        // gets all versions, current first
        public abstract IEnumerable<TEntity> GetAllVersions(int nodeId);

        // gets all version ids, current first
        public virtual IEnumerable<Guid> GetVersionIds(int nodeId, int maxRows)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersionIds", tsql =>
                tsql.Select<ContentVersionDto>(x => x.VersionId)
                    .From<ContentVersionDto>()
                    .Where<ContentVersionDto>(x => x.NodeId == SqlTemplate.ArgValue<int>("nodeId"))
                    .OrderByDescending<ContentVersionDto>(x => x.Current) // current '1' comes before others '0'
                    .AndByDescending<ContentVersionDto>(x => x.VersionDate) // most recent first
            );
            return Database.Fetch<Guid>(SqlSyntax.SelectTop(template.Sql(nodeId), maxRows));
        }

        // deletes a specific version
        public virtual void DeleteVersion(Guid versionId)
        {
            // fixme test object node type?

            // get the version we want to delete
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersion", tsql =>
                tsql.Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.VersionId == SqlTemplate.ArgValue<Guid>("versionId"))
            );
            var versionDto = Database.Fetch<ContentVersionDto>(template.Sql(versionId)).FirstOrDefault();

            // nothing to delete
            if (versionDto == null)
                return;

            // don't delete the current version
            if (versionDto.Current)
                throw new InvalidOperationException("Cannot delete the current version.");

            PerformDeleteVersion(versionDto.NodeId, versionId);
        }

        // deletes all version older than a date
        public virtual void DeleteVersions(int nodeId, DateTime versionDate)
        {
            // fixme test object node type?

            // get the versions we want to delete, excluding the current one
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetVersion", tsql =>
                tsql.Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.NodeId == SqlTemplate.ArgValue<int>("nodeId") && !x.Current && x.VersionDate < SqlTemplate.ArgValue<DateTime>("date"))
            );
            var versionDtos = Database.Fetch<ContentVersionDto>(template.Sql(nodeId, versionDate)); // fixme ok params?
            foreach (var versionDto in versionDtos)
                PerformDeleteVersion(versionDto.NodeId, versionDto.VersionId);
        }

        // actually deletes a version
        protected abstract void PerformDeleteVersion(int id, Guid versionId);

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
        /// Clears tags for an item.
        /// </summary>
        protected void ClearEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            tagRepo.ClearTagsFromEntity(entity.Id);
        }

        /// <summary>
        /// Updates tags for an item.
        /// </summary>
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

        /// <summary>
        /// Determines if an item has a property that supports tags.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool HasTagProperty(IContentBase entity)
        {
            return entity.Properties.Any(x => x.TagSupport.Enable);
        }

        #endregion

        // sql: the main sql
        // filterSql: a filtering ? fixme different from v7?
        // orderBy: the name of an ordering field
        // orderDirection: direction for orderBy
        // orderBySystemField: whether orderBy is a system field or a custom field (property value)
        private Sql<ISqlContext> PrepareSqlForPagedResults(Sql<ISqlContext> sql, Sql<ISqlContext> filterSql, string orderBy, Direction orderDirection, bool orderBySystemField)
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
                var matches = VersionableRepositoryBaseAliasRegex.For(SqlContext.SqlSyntax).Matches(sql.SQL);
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
            var matches = VersionableRepositoryBaseAliasRegex.For(SqlContext.SqlSyntax).Matches(sql.SQL);
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
                .InnerJoin<PropertyDataDto>("opdata").On<ContentVersionDto, PropertyDataDto>((left, right) => left.NodeId == right.NodeId && left.VersionId == right.VersionId && left.Current, "cver", "opdata")
                .InnerJoin<PropertyTypeDto>("optype").On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id, "opdata", "optype")
                .Where<PropertyTypeDto>(x => x.Alias == "", "optype");

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

        protected IEnumerable<TEntity> GetPagedResultsByQuery<TDto>(IQuery<TEntity> query,
            long pageIndex, int pageSize, out long totalRecords,
            Func<List<TDto>, IEnumerable<TEntity>> mapDtos,
            string orderBy, Direction orderDirection, bool orderBySystemField,
            Sql<ISqlContext> filterSql = null) // fixme filter is different on v7?
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            // start with base query, and apply the supplied IQuery
            if (query == null) query = UnitOfWork.SqlContext.Query<TEntity>();
            var sqlNodeIds = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            sqlNodeIds = PrepareSqlForPagedResults(sqlNodeIds, filterSql, orderBy, orderDirection, orderBySystemField);

            // get a page of DTOs and the total count
            var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sqlNodeIds);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            // map the DTOs and return
            return mapDtos(pagedResult.Items);
        }

        protected IDictionary<Guid, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps)
            where T : class, IContentBase
        {
            var versions = temps.Select(x => x.VersionId).ToArray();
            if (versions.Length == 0) return new Dictionary<Guid, PropertyCollection>();

            // get all PropertyDataDto for all definitions / versions
            var allPropertyDataDtos = Database.FetchByGroups<PropertyDataDto, Guid>(versions, 2000, batch =>
                SqlContext.Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .WhereIn<PropertyDataDto>(x => x.VersionId, batch))
                .ToList();

            // get PropertyDataDto distinct PropertyTypeDto
            var allPropertyTypeIds = allPropertyDataDtos.Select(x => x.PropertyTypeId).Distinct().ToList();
            var allPropertyTypeDtos = Database.FetchByGroups<PropertyTypeDto, int>(allPropertyTypeIds, 2000, batch =>
                SqlContext.Sql()
                    .Select<PropertyTypeDto>()
                    .From<PropertyTypeDto>()
                    .WhereIn<PropertyTypeDto>(x => x.Id, batch));

            // index the types for perfs, and assign to PropertyDataDto
            var indexedPropertyTypeDtos = allPropertyTypeDtos.ToDictionary(x => x.Id, x => x);
            foreach (var a in allPropertyDataDtos)
                a.PropertyTypeDto = indexedPropertyTypeDtos[a.PropertyTypeId];

            // lazy access to prevalue for data types if any property requires tag support
            var pre = new Lazy<IEnumerable<DataTypePreValueDto>>(() =>
            {
                return Database.FetchByGroups<DataTypePreValueDto, int>(allPropertyTypeIds, 2000, batch =>
                    SqlContext.Sql()
                        .Select<DataTypePreValueDto>()
                        .From<DataTypePreValueDto>()
                        .WhereIn<DataTypePreValueDto>(x => x.DataTypeNodeId, batch));
            });

            // now we have
            // - the definitinos
            // - all property data dtos
            // - a lazy access to prevalues
            // and we need to build the proper property collections

            return GetPropertyCollections(temps, allPropertyDataDtos, pre);
        }

        private IDictionary<Guid, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps, IEnumerable<PropertyDataDto> allPropertyDataDtos, Lazy<IEnumerable<DataTypePreValueDto>> allPreValues)
            where T : class, IContentBase
        {
            var result = new Dictionary<Guid, PropertyCollection>();
            var propertiesWithTagSupport = new Dictionary<string, SupportTagsAttribute>();
            var compositionPropertiesIndex = new Dictionary<int, PropertyType[]>();

            // index PropertyDataDto per versionId for perfs
            var indexedPropertyDataDtos = new Dictionary<Guid, List<PropertyDataDto>>();
            foreach (var dto in allPropertyDataDtos)
            {
                var version = dto.VersionId;
                if (indexedPropertyDataDtos.TryGetValue(version, out var list) == false)
                    indexedPropertyDataDtos[version] = list = new List<PropertyDataDto>();
                list.Add(dto);
            }

            foreach (var temp in temps)
            {
                // compositionProperties is the property types for the entire composition
                // use an index for perfs
                if (compositionPropertiesIndex.TryGetValue(temp.ContentType.Id, out var compositionProperties) == false)
                    compositionPropertiesIndex[temp.ContentType.Id] = compositionProperties = temp.ContentType.CompositionPropertyTypes.ToArray();

                // map the list of PropertyDataDto to a list of Property
                var properties = indexedPropertyDataDtos.TryGetValue(temp.VersionId, out var propertyDataDtos)
                    ? PropertyFactory.BuildEntities(propertyDataDtos, compositionProperties).ToList()
                    : new List<Property>();

                // deal with tags
                Dictionary<string, object> additionalData = null;
                foreach (var property in properties)
                {
                    // test for support and cache
                    var editor = Current.PropertyEditors[property.PropertyType.PropertyEditorAlias];
                    if (propertiesWithTagSupport.TryGetValue(property.PropertyType.PropertyEditorAlias, out var tagSupport) == false)
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
                    // fixme this is totally borked of course for variants
                    var contentPropData = new ContentPropertyData(property.GetValue(), preVals, additionalData);
                    TagExtractor.SetPropertyTags(property, contentPropData, property.GetValue(), tagSupport);
                }

                if (result.ContainsKey(temp.VersionId))
                {
                    var msg = $"The query returned multiple property sets for content {temp.Id}, {temp.ContentType.Name}";
                    if (VersionableRepositoryBase.ThrowOnWarning)
                        throw new InvalidOperationException(msg);
                    Logger.Warn<VersionableRepositoryBase<TId, TEntity, TRepository>>(msg);
                }

                result[temp.VersionId] = new PropertyCollection(properties);
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
        (" + string.Format(parsedOriginalSql, "uContent.contentTypeId") + @") as docData
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
            var propSql = new Sql(@"SELECT " + Constants.DatabaseSchema.Tables.PropertyData + ".*
FROM " + Constants.DatabaseSchema.Tables.PropertyData + "
INNER JOIN cmsPropertyType
ON " + Constants.DatabaseSchema.Tables.PropertyData + ".propertytypeid = cmsPropertyType.id
INNER JOIN
    (" + string.Format(parsedOriginalSql, "uContent.nodeId, cmsContentVersion.VersionId") + @") as docData
ON " + Constants.DatabaseSchema.Tables.PropertyData + ".versionId = docData.VersionId AND " + Constants.DatabaseSchema.Tables.PropertyData + ".nodeId = docData.nodeId
ORDER BY nodeId, versionId, propertytypeid
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
            return SqlContext.SqlSyntax.GetQuotedTableName(tableName) + "." + SqlContext.SqlSyntax.GetQuotedColumnName(fieldName);
        }

        #region UnitOfWork Events

        public class UnitOfWorkEntityEventArgs : EventArgs
        {
            public UnitOfWorkEntityEventArgs(IScopeUnitOfWork unitOfWork, TEntity entity)
            {
                UnitOfWork = unitOfWork;
                Entity = entity;
            }

            public IScopeUnitOfWork UnitOfWork { get; }
            public TEntity Entity { get; }
        }

        public class UnitOfWorkVersionEventArgs : EventArgs
        {
            public UnitOfWorkVersionEventArgs(IScopeUnitOfWork unitOfWork, int entityId, Guid versionId)
            {
                UnitOfWork = unitOfWork;
                EntityId = entityId;
                VersionId = versionId;
            }

            public IScopeUnitOfWork UnitOfWork { get; }
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
            public TempContent(int id, Guid versionId, IContentTypeComposition contentType)
            {
                Id = id;
                VersionId = versionId;
                ContentType = contentType;
            }

            /// <summary>
            /// Gets or sets the identifier of the content.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the version identifier of the content.
            /// </summary>
            public Guid VersionId { get; set; }

            /// <summary>
            /// Gets or sets the content type.
            /// </summary>
            public IContentTypeComposition ContentType { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the template of the content.
            /// </summary>
            public int? TemplateId { get; set; }
        }

        protected class TempContent<T> : TempContent
            where T : class, IContentBase
        {
            public TempContent(int id, Guid versionId, IContentTypeComposition contentType, T content = null)
                : base(id, versionId, contentType)
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
                .Select<NodeDto>(x => x.NodeId, x => x.Text)
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.ArgValue<Guid>("nodeObjectType") && x.ParentId == SqlTemplate.ArgValue<int>("parentId")));

            var sql = template.Sql(NodeObjectTypeId, parentId);
            var names = Database.Fetch<SimilarNodeName>(sql);

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        protected virtual int GetNewChildSortOrder(int parentId, int first)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetSortOrder", tsql =>
                tsql.Select($"COALESCE(MAX(sortOrder),{first - 1})").From<NodeDto>().Where<NodeDto>(x => x.ParentId == SqlTemplate.ArgValue<int>("parentId") && x.NodeObjectType == NodeObjectTypeId)
            );

            return Database.ExecuteScalar<int>(template.Sql(new { parentId })) + 1;
        }

        protected virtual NodeDto GetParentNodeDto(int parentId)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetParentNode", tsql =>
                tsql.Select<NodeDto>().From<NodeDto>().Where<NodeDto>(x => x.NodeId == SqlTemplate.ArgValue<int>("parentId"))
            );

            return Database.Fetch<NodeDto>(template.Sql(parentId)).First();
        }

        protected virtual int GetReservedId(Guid uniqueId)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.VersionableRepository.GetReservedId", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).From<NodeDto>().Where<NodeDto>(x => x.UniqueId == SqlTemplate.ArgValue<Guid>("uniqueId") && x.NodeObjectType == Constants.ObjectTypes.IdReservation)
            );
            var id = Database.ExecuteScalar<int?>(template.Sql(new { uniqueId = uniqueId }));
            return id ?? 0;
        }

        #endregion
    }
}
