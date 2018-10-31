using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Media;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class VersionableRepositoryBase<TId, TEntity> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
    {
        private readonly IContentSection _contentSection;

        /// <summary>
        /// This is used for unit tests ONLY
        /// </summary>
        internal static bool ThrowOnWarning = false;

        protected VersionableRepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IContentSection contentSection)
            : base(work, cache, logger, sqlSyntax)
        {
            _contentSection = contentSection;
        }

        #region IRepositoryVersionable Implementation

        /// <summary>
        /// Gets a list of all versions for an <see cref="TEntity"/> ordered so latest is first
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> to retrieve versions from</param>
        /// <returns>An enumerable list of the same <see cref="TEntity"/> object with different versions</returns>
        public abstract IEnumerable<TEntity> GetAllVersions(int id);

        /// <summary>
        /// Gets a list of all version Ids for the given content item ordered so latest is first
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maxRows">The maximum number of rows to return</param>
        /// <returns></returns>
        public virtual IEnumerable<Guid> GetVersionIds(int id, int maxRows)
        {
            var sql = new Sql();
            sql.Select("cmsDocument.versionId")
                .From<DocumentDto>(SqlSyntax)
                .InnerJoin<ContentDto>(SqlSyntax)
                .On<DocumentDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<NodeDto>(x => x.NodeId == id)
                .OrderByDescending<DocumentDto>(x => x.UpdateDate, SqlSyntax);

            return Database.Fetch<Guid>(SqlSyntax.SelectTop(sql, maxRows));
        }

        public virtual void DeleteVersion(Guid versionId)
        {
            var dto = Database.FirstOrDefault<ContentVersionDto>("WHERE versionId = @VersionId", new { VersionId = versionId });
            if (dto == null) return;

            //Ensure that the lastest version is not deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = dto.NodeId });
            if (latestVersionDto.VersionId == dto.VersionId)
                return;

            using (var transaction = Database.GetTransaction())
            {
                PerformDeleteVersion(dto.NodeId, versionId);

                transaction.Complete();
            }
        }

        public virtual void DeleteVersions(int id, DateTime versionDate)
        {
            //Ensure that the latest version is not part of the versions being deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = id });
            var list =
                Database.Fetch<ContentVersionDto>(
                    "WHERE versionId <> @VersionId AND (ContentId = @Id AND VersionDate < @VersionDate)",
                    new { VersionId = latestVersionDto.VersionId, Id = id, VersionDate = versionDate });
            if (list.Any() == false) return;

            using (var transaction = Database.GetTransaction())
            {
                foreach (var dto in list)
                {
                    PerformDeleteVersion(id, dto.VersionId);
                }

                transaction.Complete();
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

        /// <summary>
        /// Gets paged document descendants as XML by path
        /// </summary>
        /// <param name="path">Path starts with</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="orderBy"></param>
        /// <param name="totalRecords">Total records the query would return without paging</param>
        /// <returns>A paged enumerable of XML entries of content items</returns>
        public virtual IEnumerable<XElement> GetPagedXmlEntriesByPath(string path, long pageIndex, int pageSize, string[] orderBy, out long totalRecords)
        {
            var query = new Sql().Select(string.Format("umbracoNode.id, cmsContentXml.{0}", SqlSyntax.GetQuotedColumnName("xml")))
                .From("umbracoNode")
                .InnerJoin("cmsContentXml").On("cmsContentXml.nodeId = umbracoNode.id");

            if (path == "-1")
            {
                query.Where("umbracoNode.nodeObjectType = @type", new { type = NodeObjectTypeId });
            }
            else
            {
                query.Where(string.Format("umbracoNode.{0} LIKE (@0)", SqlSyntax.GetQuotedColumnName("path")), path.EnsureEndsWith(",%"));
            }

            //each order by param needs to be in a bracket! see: https://github.com/toptensoftware/PetaPoco/issues/177
            query.OrderBy(orderBy == null
                ? "(umbracoNode.id)"
                : string.Join(",", orderBy.Select(x => string.Format("({0})", SqlSyntax.GetQuotedColumnName(x)))));

            var pagedResult = Database.Page<ContentXmlDto>(pageIndex + 1, pageSize, query);
            totalRecords = pagedResult.TotalItems;
            return pagedResult.Items.Select(dto => XElement.Parse(dto.Xml));
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            var pathMatch = parentId == -1
                ? "-1,"
                : "," + parentId + ",";
            var sql = new Sql();
            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.Path.Contains(pathMatch));
            }
            else
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
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
            var sql = new Sql();
            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .Where<NodeDto>(x => x.ParentId == parentId);
            }
            else
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
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
            var sql = new Sql();
            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            }
            else
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
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
        protected void UpdatePropertyTags(IContentBase entity, ITagRepository tagRepo)
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

        private Sql GetFilteredSqlForPagedResults(Sql sql, Func<Tuple<string, object[]>> defaultFilter = null)
        {
            Sql filteredSql;

            // Apply filter
            if (defaultFilter != null)
            {
                //NOTE: It is assumed here that the `sql` already contains a WHERE clause, see UserRepository.GetFilteredSqlForPagedResults
                // for an example of when it's not assumed there's already a WHERE clause

                var filterResult = defaultFilter();

                //NOTE: this is certainly strange - NPoco handles this much better but we need to re-create the sql
                // instance a couple of times to get the parameter order correct, for some reason the first
                // time the arguments don't show up correctly but the SQL argument parameter names are actually updated
                // accordingly - so we re-create it again. In v8 we don't need to do this and it's already taken care of.

                filteredSql = new Sql(sql.SQL, sql.Arguments);
                var args = filteredSql.Arguments.Concat(filterResult.Item2).ToArray();
                filteredSql = new Sql(
                    string.Format("{0} {1}", filteredSql.SQL, filterResult.Item1),
                    args);
                filteredSql = new Sql(filteredSql.SQL, args);
            }
            else
            {
                //copy to var so that the original isn't changed
                filteredSql = new Sql(sql.SQL, sql.Arguments);
            }
            return filteredSql;
        }

        private Sql GetSortedSqlForPagedResults(Sql sql, Direction orderDirection, string orderBy, bool orderBySystemField, Tuple<string, string> nodeIdSelect)
        {

            //copy to var so that the original isn't changed
            var sortedSql = new Sql(sql.SQL, sql.Arguments);

            if (orderBySystemField)
            {
                // Apply order according to parameters
                if (string.IsNullOrEmpty(orderBy) == false)
                {
                    var orderByParams = new[] { GetDatabaseFieldNameForOrderBy(orderBy) };
                    if (orderDirection == Direction.Ascending)
                    {
                        sortedSql.OrderBy(orderByParams);
                    }
                    else
                    {
                        sortedSql.OrderByDescending(orderByParams);
                    }
                }
            }
            else
            {
                // Sorting by a custom field, so set-up sub-query for ORDER BY clause to pull through value
                // from most recent content version for the given order by field
                var sortedInt = string.Format(SqlSyntax.ConvertIntegerToOrderableString, "dataInt");
                var sortedDate = string.Format(SqlSyntax.ConvertDateToOrderableString, "dataDate");
                var sortedString = string.Format("COALESCE({0},'')", "dataNvarchar");
                var sortedDecimal = string.Format(SqlSyntax.ConvertDecimalToOrderableString, "dataDecimal");

                //these are defaults that will be used in the query - they can be overridden for non-versioned entities or document entities
                var versionQuery = " AND cpd.versionId = cd.versionId";
                var newestQuery = string.Empty;

                //cmsDocument needs to filter by the 'newest' parameter in the query
                if (nodeIdSelect.Item1 == "cmsDocument")
                    newestQuery = " AND cd.newest = 1";

                //members do not use versions so clear the versionQuery string
                if (nodeIdSelect.Item1 == "cmsMember")
                    versionQuery = string.Empty;

                //needs to be an outer join since there's no guarantee that any of the nodes have values for this property
                var outerJoinTempTable = string.Format(@"LEFT OUTER JOIN (
                                SELECT CASE
                                    WHEN dataInt Is Not Null THEN {0}
                                    WHEN dataDecimal Is Not Null THEN {1}
                                    WHEN dataDate Is Not Null THEN {2}
                                    ELSE {3}
                                END AS CustomPropVal,
                                cd.{4} AS CustomPropValContentId
                                FROM {5} cd
                                INNER JOIN cmsPropertyData cpd ON cpd.contentNodeId = cd.{4}{6}
                                INNER JOIN cmsPropertyType cpt ON cpt.Id = cpd.propertytypeId
                                WHERE cpt.Alias = @{7}{8}) AS CustomPropData
                                ON CustomPropData.CustomPropValContentId = umbracoNode.id
                ", sortedInt, sortedDecimal, sortedDate, sortedString, nodeIdSelect.Item2, nodeIdSelect.Item1, versionQuery, sortedSql.Arguments.Length, newestQuery);

                //insert this just above the last WHERE
                string newSql = sortedSql.SQL.Insert(sortedSql.SQL.LastIndexOf("WHERE"), outerJoinTempTable);

                var newArgs = sortedSql.Arguments.ToList();
                newArgs.Add(orderBy);

                sortedSql = new Sql(newSql, newArgs.ToArray());

                if (orderDirection == Direction.Descending)
                {
                    sortedSql.OrderByDescending("CustomPropData.CustomPropVal");
                    // need to ensure ordering unique by using id as CustomPropVal may not be unique
                    // see: https://github.com/umbraco/Umbraco-CMS/issues/3296
                    sortedSql.OrderByDescending("umbracoNode.id");
                }
                else
                {
                    sortedSql.OrderBy("CustomPropData.CustomPropVal");
                    // need to ensure ordering unique by using id as CustomPropVal may not be unique
                    // see: https://github.com/umbraco/Umbraco-CMS/issues/3296
                    sortedSql.OrderBy("umbracoNode.id");
                }
            }

            if (orderBySystemField && orderBy != "umbracoNode.id")
            {
                //no matter what we always MUST order the result also by umbracoNode.id to ensure that all records being ordered by are unique.
                // if we do not do this then we end up with issues where we are ordering by a field that has duplicate values (i.e. the 'text' column
                // is empty for many nodes)
                // see: http://issues.umbraco.org/issue/U4-8831
                sortedSql.OrderBy("umbracoNode.id");
            }

            return sortedSql;

        }

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
            if (orderBy == null) throw new ArgumentNullException("orderBy");

            // Get base query for returning IDs
            var sqlBaseIds = GetBaseQuery(BaseQueryType.Ids);
            // Get base query for returning all data
            var sqlBaseFull = GetBaseQuery(BaseQueryType.FullMultiple);

            if (query == null) query = new Query<TEntity>();
            var translatorIds = new SqlTranslator<TEntity>(sqlBaseIds, query);
            var sqlQueryIds = translatorIds.Translate();
            var translatorFull = new SqlTranslator<TEntity>(sqlBaseFull, query);
            var sqlQueryFull = translatorFull.Translate();

            //get sorted and filtered sql
            var sqlNodeIdsWithSort = GetSortedSqlForPagedResults(
                GetFilteredSqlForPagedResults(sqlQueryIds, defaultFilter),
                orderDirection, orderBy, orderBySystemField, nodeIdSelect);

            // Get page of results and total count
            IEnumerable<TEntity> result;
            var pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sqlNodeIdsWithSort);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            //NOTE: We need to check the actual items returned, not the 'totalRecords', that is because if you request a page number
            // that doesn't actually have any data on it, the totalRecords will still indicate there are records but there are none in
            // the pageResult, then the GetAll will actually return ALL records in the db.
            if (pagedResult.Items.Any())
            {
                //Create the inner paged query that was used above to get the paged result, we'll use that as the inner sub query
                var args = sqlNodeIdsWithSort.Arguments;
                string sqlStringCount, sqlStringPage;
                Database.BuildPageQueries<TDto>(pageIndex * pageSize, pageSize, sqlNodeIdsWithSort.SQL, ref args, out sqlStringCount, out sqlStringPage);

                //We need to make this FULL query an inner join on the paged ID query
                var splitQuery = sqlQueryFull.SQL.Split(new[] { "WHERE " }, StringSplitOptions.None);
                var fullQueryWithPagedInnerJoin = new Sql(splitQuery[0])
                    .Append("INNER JOIN (")
                    //join the paged query with the paged query arguments
                    .Append(sqlStringPage, args)
                    .Append(") temp ")
                    .Append(string.Format("ON {0}.{1} = temp.{1}", nodeIdSelect.Item1, nodeIdSelect.Item2))
                    //add the original where clause back with the original arguments
                    .Where(splitQuery[1], sqlQueryIds.Arguments);

                //get sorted and filtered sql
                var fullQuery = GetSortedSqlForPagedResults(
                    GetFilteredSqlForPagedResults(fullQueryWithPagedInnerJoin, defaultFilter),
                    orderDirection, orderBy, orderBySystemField, nodeIdSelect);
                
                return processQuery(fullQuery, new PagingSqlQuery<TDto>(Database, sqlNodeIdsWithSort, pageIndex, pageSize));
            }
            else
            {
                result = Enumerable.Empty<TEntity>();
            }

            return result;
        }

        /// <summary>
        /// Gets the property collection for a non-paged query
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="documentDefs"></param>
        /// <returns></returns>
        protected IDictionary<Guid, PropertyCollection> GetPropertyCollection(
            Sql sql,
            IReadOnlyCollection<DocumentDefinition> documentDefs)
        {
            return GetPropertyCollection(new PagingSqlQuery(sql), documentDefs);
        }

        /// <summary>
        /// Gets the property collection for a query
        /// </summary>
        /// <param name="pagingSqlQuery"></param>
        /// <param name="documentDefs"></param>
        /// <returns></returns>
        protected IDictionary<Guid, PropertyCollection> GetPropertyCollection(
            PagingSqlQuery pagingSqlQuery,
            IReadOnlyCollection<DocumentDefinition> documentDefs)
        {
            if (documentDefs.Count == 0) return new Dictionary<Guid, PropertyCollection>();

            //initialize to the query passed in
            var docSql = pagingSqlQuery.PrePagedSql;

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
                        var editor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);

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

                            var contentPropData = new ContentPropertyData(property.Value, preVals);

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
                            Logger.Warn<VersionableRepositoryBase<TId, TEntity>>(msg);
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
        
        protected virtual string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            // Translate the passed order by field (which were originally defined for in-memory object sorting
            // of ContentItemBasic instances) to the database field names.
            switch (orderBy.ToUpperInvariant())
            {
                case "UPDATEDATE":
                    return "cmsContentVersion.VersionDate";
                case "NAME":
                    return "umbracoNode.text";
                case "PUBLISHED":
                    return "cmsDocument.published";
                case "OWNER":
                    //TODO: This isn't going to work very nicely because it's going to order by ID, not by letter
                    return "umbracoNode.nodeUser";
                // Members only
                case "USERNAME":
                    return "cmsMember.LoginName";
                default:
                    //ensure invalid SQL cannot be submitted
                    return Regex.Replace(orderBy, @"[^\w\.,`\[\]@-]", "");
            }
        }

        protected virtual string GetEntityPropertyNameForOrderBy(string orderBy)
        {
            // Translate the passed order by field (which were originally defined for in-memory object sorting
            // of ContentItemBasic instances) to the IMedia property names.
            switch (orderBy.ToUpperInvariant())
            {
                case "OWNER":
                    //TODO: This isn't going to work very nicely because it's going to order by ID, not by letter
                    return "CreatorId";
                case "UPDATER":
                    //TODO: This isn't going to work very nicely because it's going to order by ID, not by letter
                    return "WriterId";
                case "VERSIONDATE":
                    return "UpdateDate";
                default:
                    //ensure invalid SQL cannot be submitted
                    return Regex.Replace(orderBy, @"[^\w\.,`\[\]@-]", "");
            }
        }

        /// <summary>
        /// Deletes all media files passed in.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual bool DeleteMediaFiles(IEnumerable<string> files)
        {
            //ensure duplicates are removed
            files = files.Distinct();

            var allsuccess = true;

            var fs = FileSystemProviderManager.Current.MediaFileSystem;
            Parallel.ForEach(files, file =>
            {
                try
                {
                    if (file.IsNullOrWhiteSpace()) return;

                    var relativeFilePath = fs.GetRelativePath(file);
                    if (fs.FileExists(relativeFilePath) == false) return;

                    var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (_contentSection.UploadAllowDirectories && parentDirectory != fs.GetRelativePath("/"))
                    {
                        //issue U4-771: if there is a parent directory the recursive parameter should be true
                        fs.DeleteDirectory(parentDirectory, String.IsNullOrEmpty(parentDirectory) == false);
                    }
                    else
                    {
                        fs.DeleteFile(file, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error<VersionableRepositoryBase<TId, TEntity>>("An error occurred while deleting file attached to nodes: " + file, e);
                    allsuccess = false;
                }
            });

            return allsuccess;
        }

        /// <summary>
        /// For Paging, repositories must support returning different query for the query type specified
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        protected abstract Sql GetBaseQuery(BaseQueryType queryType);

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
                    base.Add(item);
                    return true;
                }

                if (Dictionary == null)
                {
                    base.Add(item);
                    return true;
                }

                var key = GetKeyForItem(item);
                DocumentDefinition found;
                if (TryGetValue(key, out found))
                {
                    //it already exists and it's older so we need to replace it
                    if (item.VersionId > found.VersionId)
                    {
                        var currIndex = Items.IndexOf(found);
                        if (currIndex == -1)
                            throw new IndexOutOfRangeException("Could not find the item in the list: " + found.Version);

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
          
            public bool TryGetValue(ValueType key, out DocumentDefinition val)
            {
                if (Dictionary == null)
                {
                    val = null;
                    return false;
                }
                return Dictionary.TryGetValue(key, out val);
            }
        }

        /// <summary>
        /// A custom comparer required for sorting entities by GUIDs to match how the sorting of GUIDs works on SQL server
        /// </summary>
        /// <remarks>
        /// MySql sorts GUIDs as a string, MSSQL sorts based on byte sections, this comparer will allow sorting GUIDs to be the same as how SQL server does
        /// </remarks>
        private class DocumentDefinitionComparer : IComparer<Guid>
        {
            private readonly ISqlSyntaxProvider _sqlSyntax;

            public DocumentDefinitionComparer(ISqlSyntaxProvider sqlSyntax)
            {
                _sqlSyntax = sqlSyntax;
            }

            public int Compare(Guid x, Guid y)
            {
                //MySql sorts on GUIDs as strings (i.e. normal)
                if (_sqlSyntax is MySqlSyntaxProvider)
                {
                    return x.CompareTo(y);
                }

                //MSSQL doesn't it sorts them on byte sections!
                return new SqlGuid(x).CompareTo(new SqlGuid(y));
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

            public DocumentDto DocumentDto { get; private set; }
            public ContentVersionDto ContentVersionDto { get; private set; }

            public int Id
            {
                get { return ContentVersionDto.NodeId; }
            }
            
            public Guid Version
            {
                get { return DocumentDto != null ? DocumentDto.VersionId : ContentVersionDto.VersionId; }
            }

            /// <summary>
            /// This is used to determien which version is the most recent
            /// </summary>
            public int VersionId
            {
                get { return ContentVersionDto.Id; }
            }

            public DateTime VersionDate
            {
                get { return ContentVersionDto.VersionDate; }
            }

            public DateTime CreateDate
            {
                get { return ContentVersionDto.ContentDto.NodeDto.CreateDate; }
            }

            public IContentTypeComposition Composition { get; set; }            

            
        }

        /// <summary>
        /// An object representing a query that may contain paging information
        /// </summary>
        internal class PagingSqlQuery
        {
            public Sql PrePagedSql { get; private set; }

            public PagingSqlQuery(Sql prePagedSql)
            {
                PrePagedSql = prePagedSql;
            }

            public virtual bool HasPaging
            {
                get { return false; }
            }

            public virtual Sql BuildPagedQuery(string selectColumns)
            {
                throw new InvalidOperationException("This query has no paging information");
            }
        }

        /// <summary>
        /// An object representing a query that contains paging information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class PagingSqlQuery<T> : PagingSqlQuery
        {
            private readonly Database _db;
            private readonly long _pageIndex;
            private readonly int _pageSize;

            public PagingSqlQuery(Database db, Sql prePagedSql, long pageIndex, int pageSize) : base(prePagedSql)
            {
                _db = db;
                _pageIndex = pageIndex;
                _pageSize = pageSize;                
            }

            public override bool HasPaging
            {
                get { return _pageSize > 0; }
            }

            /// <summary>
            /// Creates a paged query based on the original query and subtitutes the selectColumns specified
            /// </summary>
            /// <param name="selectColumns"></param>
            /// <returns></returns>
            public override Sql BuildPagedQuery(string selectColumns)
            {
                if (HasPaging == false) throw new InvalidOperationException("This query has no paging information");

                var resultSql = string.Format("SELECT {0} {1}", selectColumns, PrePagedSql.SQL.Substring(PrePagedSql.SQL.IndexOf("FROM", StringComparison.Ordinal)));

                //this query is meant to be paged so we need to generate the paging syntax
                //Create the inner paged query that was used above to get the paged result, we'll use that as the inner sub query
                var args = PrePagedSql.Arguments;
                string sqlStringCount, sqlStringPage;
                _db.BuildPageQueries<T>(_pageIndex * _pageSize, _pageSize, resultSql, ref args, out sqlStringCount, out sqlStringPage);

                return new Sql(sqlStringPage, args);
            }
        }
    }
}
