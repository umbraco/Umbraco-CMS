using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal sealed class ContentRepositoryBase
    {
        /// <summary>
        /// This is used for unit tests ONLY
        /// </summary>
        public static bool ThrowOnWarning { get; set; } = false;
    }

    public abstract class ContentRepositoryBase<TId, TEntity, TRepository> : EntityRepositoryBase<TId, TEntity>, IContentRepository<TId, TEntity>
        where TEntity : class, IContentBase
        where TRepository : class, IRepository
    {
        private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactories;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        ///
        /// </summary>
        /// <param name="scopeAccessor"></param>
        /// <param name="cache"></param>
        /// <param name="logger"></param>
        /// <param name="languageRepository"></param>
        /// <param name="relationRepository"></param>
        /// <param name="relationTypeRepository"></param>
        /// <param name="dataValueReferenceFactories"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="propertyEditors">
        ///     Lazy property value collection - must be lazy because we have a circular dependency since some property editors require services, yet these services require property editors
        /// </param>
        protected ContentRepositoryBase(
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<EntityRepositoryBase<TId, TEntity>> logger,
            ILanguageRepository languageRepository,
            IRelationRepository relationRepository,
            IRelationTypeRepository relationTypeRepository,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeService dataTypeService,
            IEventAggregator eventAggregator)
            : base(scopeAccessor, cache, logger)
        {
            DataTypeService = dataTypeService;
            LanguageRepository = languageRepository;
            RelationRepository = relationRepository;
            RelationTypeRepository = relationTypeRepository;
            PropertyEditors = propertyEditors;
            _dataValueReferenceFactories = dataValueReferenceFactories;
            _eventAggregator = eventAggregator;
        }

        protected abstract TRepository This { get; }

        /// <summary>
        /// Gets the node object type for the repository's entity
        /// </summary>
        protected abstract Guid NodeObjectTypeId { get; }

        protected ILanguageRepository LanguageRepository { get; }

        protected IDataTypeService DataTypeService { get; }

        protected IRelationRepository RelationRepository { get; }

        protected IRelationTypeRepository RelationTypeRepository { get; }

        protected PropertyEditorCollection PropertyEditors { get; }

        #region Versions

        // gets a specific version
        public abstract TEntity? GetVersion(int versionId);

        // gets all versions, current first
        public abstract IEnumerable<TEntity> GetAllVersions(int nodeId);

        // gets all versions, current first
        public virtual IEnumerable<TEntity> GetAllVersionsSlim(int nodeId, int skip, int take)
            => GetAllVersions(nodeId).Skip(skip).Take(take);

        // gets all version ids, current first
        public virtual IEnumerable<int> GetVersionIds(int nodeId, int maxRows)
        {
            SqlTemplate template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.GetVersionIds, tsql =>
                tsql.Select<ContentVersionDto>(x => x.Id)
                    .From<ContentVersionDto>()
                    .Where<ContentVersionDto>(x => x.NodeId == SqlTemplate.Arg<int>("nodeId"))
                    .OrderByDescending<ContentVersionDto>(x => x.Current) // current '1' comes before others '0'
                    .AndByDescending<ContentVersionDto>(x => x.VersionDate)); // most recent first

            return Database.Fetch<int>(SqlSyntax.SelectTop(template.Sql(nodeId), maxRows));
        }

        // deletes a specific version
        public virtual void DeleteVersion(int versionId)
        {
            // TODO: test object node type?

            // get the version we want to delete
            SqlTemplate template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.GetVersion, tsql =>
                tsql.Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.Id == SqlTemplate.Arg<int>("versionId")));
            ContentVersionDto? versionDto = Database.Fetch<ContentVersionDto>(template.Sql(new { versionId })).FirstOrDefault();

            // nothing to delete
            if (versionDto == null)
            {
                return;
            }

            // don't delete the current version
            if (versionDto.Current)
            {
                throw new InvalidOperationException("Cannot delete the current version.");
            }

            PerformDeleteVersion(versionDto.NodeId, versionId);
        }

        // deletes all versions of an entity, older than a date.
        public virtual void DeleteVersions(int nodeId, DateTime versionDate)
        {
            // TODO: test object node type?

            // get the versions we want to delete, excluding the current one
            SqlTemplate template = SqlContext.Templates.Get(
                Constants.SqlTemplates.VersionableRepository.GetVersions,
                tsql =>
                tsql.Select<ContentVersionDto>()
                    .From<ContentVersionDto>()
                    .Where<ContentVersionDto>(x =>
                        x.NodeId == SqlTemplate.Arg<int>("nodeId") &&
                        !x.Current &&
                        x.VersionDate < SqlTemplate.Arg<DateTime>("versionDate")));
            List<ContentVersionDto>? versionDtos = Database.Fetch<ContentVersionDto>(template.Sql(new { nodeId, versionDate }));
            foreach (ContentVersionDto versionDto in versionDtos)
            {
                PerformDeleteVersion(versionDto.NodeId, versionDto.Id);
            }
        }

        // actually deletes a version
        protected abstract void PerformDeleteVersion(int id, int versionId);

        #endregion

        #region Count

        /// <summary>
        /// Count descendants of an item.
        /// </summary>
        public int CountDescendants(int parentId, string? contentTypeAlias = null)
        {
            var pathMatch = parentId == -1
                ? "-1,"
                : "," + parentId + ",";

            Sql<ISqlContext> sql = SqlContext.Sql()
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
        public int CountChildren(int parentId, string? contentTypeAlias = null)
        {
            Sql<ISqlContext> sql = SqlContext.Sql()
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
        public int Count(string? contentTypeAlias = null)
        {
            Sql<ISqlContext> sql = SqlContext.Sql()
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
        protected void SetEntityTags(IContentBase entity, ITagRepository tagRepo, IJsonSerializer serializer)
        {
            foreach (IProperty property in entity.Properties)
            {
                TagConfiguration? tagConfiguration = property.GetTagConfiguration(PropertyEditors, DataTypeService);
                if (tagConfiguration == null)
                {
                    continue; // not a tags property
                }

                if (property.PropertyType.VariesByCulture())
                {
                    var tags = new List<ITag>();
                    foreach (IPropertyValue pvalue in property.Values)
                    {
                        IEnumerable<string> tagsValue = property.GetTagsValue(PropertyEditors, DataTypeService, serializer, pvalue.Culture);
                        var languageId = LanguageRepository.GetIdByIsoCode(pvalue.Culture);
                        IEnumerable<Tag> cultureTags = tagsValue.Select(x => new Tag { Group = tagConfiguration.Group, Text = x, LanguageId = languageId });
                        tags.AddRange(cultureTags);
                    }

                    tagRepo.Assign(entity.Id, property.PropertyTypeId, tags);
                }
                else
                {
                    IEnumerable<string> tagsValue = property.GetTagsValue(PropertyEditors, DataTypeService, serializer); // strings
                    IEnumerable<Tag> tags = tagsValue.Select(x => new Tag { Group = tagConfiguration.Group, Text = x });
                    tagRepo.Assign(entity.Id, property.PropertyTypeId, tags);
                }
            }
        }

        // TODO: should we do it when un-publishing? or?

        /// <summary>
        /// Clears tags for an item.
        /// </summary>
        protected void ClearEntityTags(IContentBase entity, ITagRepository tagRepo)
        {
            tagRepo.RemoveAll(entity.Id);
        }

        #endregion

        private Sql<ISqlContext> PreparePageSql(Sql<ISqlContext> sql, Sql<ISqlContext>? filterSql, Ordering ordering)
        {
            // non-filtering, non-ordering = nothing to do
            if (filterSql == null && ordering.IsEmpty)
            {
                return sql;
            }

            // preserve original
            var psql = new Sql<ISqlContext>(sql.SqlContext, sql.SQL, sql.Arguments);

            // apply filter
            if (filterSql != null)
            {
                psql.Append(filterSql);
            }

            // non-sorting, we're done
            if (ordering.IsEmpty)
            {
                return psql;
            }

            // else apply ordering
            ApplyOrdering(ref psql, ordering);

            // no matter what we always MUST order the result also by umbracoNode.id to ensure that all records being ordered by are unique.
            // if we do not do this then we end up with issues where we are ordering by a field that has duplicate values (i.e. the 'text' column
            // is empty for many nodes) - see: http://issues.umbraco.org/issue/U4-8831
            var (dbfield, _) = SqlContext.VisitDto<NodeDto>(x => x.NodeId);
            if (ordering.IsCustomField || !ordering.OrderBy.InvariantEquals("id"))
            {
                psql.OrderBy(GetAliasedField(dbfield, sql));
            }

            // create prepared sql
            // ensure it's single-line as NPoco PagingHelper has issues with multi-lines
            psql = Sql(psql.SQL.ToSingleLine(), psql.Arguments);

            // replace the magic culture parameter (see DocumentRepository.GetBaseQuery())
            if (!ordering.Culture.IsNullOrWhiteSpace())
            {
                for (var i = 0; i < psql.Arguments.Length; i++)
                {
                    if (psql.Arguments[i] is string s && s == "[[[ISOCODE]]]")
                    {
                        psql.Arguments[i] = ordering.Culture;
                    }
                }
            }

            return psql;
        }

        private void ApplyOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            if (ordering == null)
            {
                throw new ArgumentNullException(nameof(ordering));
            }

            var orderBy = ordering.IsCustomField
                ? ApplyCustomOrdering(ref sql, ordering)
                : ApplySystemOrdering(ref sql, ordering);

            // beware! NPoco paging code parses the query to isolate the ORDER BY fragment,
            // using a regex that wants "([\w\.\[\]\(\)\s""`,]+)" - meaning that anything
            // else in orderBy is going to break NPoco / not be detected

            // beware! NPoco paging code (in PagingHelper) collapses everything [foo].[bar]
            // to [bar] only, so we MUST use aliases, cannot use [table].[field]

            // beware! pre-2012 SqlServer is using a convoluted syntax for paging, which
            // includes "SELECT ROW_NUMBER() OVER (ORDER BY ...) poco_rn FROM SELECT (...",
            // so anything added here MUST also be part of the inner SELECT statement, ie
            // the original statement, AND must be using the proper alias, as the inner SELECT
            // will hide the original table.field names entirely
            if (ordering.Direction == Direction.Ascending)
            {
                sql.OrderBy(orderBy);
            }
            else
            {
                sql.OrderByDescending(orderBy);
            }
        }

        protected virtual string ApplySystemOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
        {
            // id is invariant
            if (ordering.OrderBy.InvariantEquals("id"))
            {
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.NodeId), sql);
            }

            // sort order is invariant
            if (ordering.OrderBy.InvariantEquals("sortOrder"))
            {
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.SortOrder), sql);
            }

            // path is invariant
            if (ordering.OrderBy.InvariantEquals("path"))
            {
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.Path), sql);
            }

            // note: 'owner' is the user who created the item as a whole,
            //       we don't have an 'owner' per culture (should we?)
            if (ordering.OrderBy.InvariantEquals("owner"))
            {
                Sql<ISqlContext> joins = Sql()
                    .InnerJoin<UserDto>("ownerUser").On<NodeDto, UserDto>((node, user) => node.UserId == user.Id, aliasRight: "ownerUser");

                // see notes in ApplyOrdering: the field MUST be selected + aliased
                sql = Sql(InsertBefore(sql, "FROM", ", " + SqlSyntax.GetFieldName<UserDto>(x => x.UserName, "ownerUser") + " AS ordering "), sql.Arguments);

                sql = InsertJoins(sql, joins);

                return "ordering";
            }

            // note: each version culture variation has a date too,
            //       maybe we would want to use it instead?
            if (ordering.OrderBy.InvariantEquals("versionDate") || ordering.OrderBy.InvariantEquals("updateDate"))
            {
                return GetAliasedField(SqlSyntax.GetFieldName<ContentVersionDto>(x => x.VersionDate), sql);
            }

            // create date is invariant (we don't keep each culture's creation date)
            if (ordering.OrderBy.InvariantEquals("createDate"))
            {
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.CreateDate), sql);
            }

            // name is variant
            if (ordering.OrderBy.InvariantEquals("name"))
            {
                // no culture = can only work on the invariant name
                // see notes in ApplyOrdering: the field MUST be aliased
                if (ordering.Culture.IsNullOrWhiteSpace())
                {
                    return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.Text!), sql);
                }

                // "variantName" alias is defined in DocumentRepository.GetBaseQuery
                // TODO: what if it is NOT a document but a ... media or whatever?
                // previously, we inserted the join+select *here* so we were sure to have it,
                // but now that's not the case anymore!
                return "variantName";
            }

            // content type alias is invariant
            if (ordering.OrderBy.InvariantEquals("contentTypeAlias"))
            {
                Sql<ISqlContext> joins = Sql()
                    .InnerJoin<ContentTypeDto>("ctype").On<ContentDto, ContentTypeDto>((content, contentType) => content.ContentTypeId == contentType.NodeId, aliasRight: "ctype");

                // see notes in ApplyOrdering: the field MUST be selected + aliased
                sql = Sql(InsertBefore(sql, "FROM", ", " + SqlSyntax.GetFieldName<ContentTypeDto>(x => x.Alias!, "ctype") + " AS ordering "), sql.Arguments);

                sql = InsertJoins(sql, joins);

                return "ordering";
            }

            // previously, we'd accept anything and just sanitize it - not anymore
            throw new NotSupportedException($"Ordering by {ordering.OrderBy} not supported.");
        }

        private string ApplyCustomOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
        {
            // sorting by a custom field, so set-up sub-query for ORDER BY clause to pull through value
            // from 'current' content version for the given order by field
            var sortedInt = string.Format(SqlContext.SqlSyntax.ConvertIntegerToOrderableString, "intValue");
            var sortedDecimal = string.Format(SqlContext.SqlSyntax.ConvertDecimalToOrderableString, "decimalValue");
            var sortedDate = string.Format(SqlContext.SqlSyntax.ConvertDateToOrderableString, "dateValue");
            var sortedString = "COALESCE(varcharValue,'')"; // assuming COALESCE is ok for all syntaxes

            // needs to be an outer join since there's no guarantee that any of the nodes have values for this property
            Sql<ISqlContext> innerSql = Sql().Select($@"CASE
                            WHEN intValue IS NOT NULL THEN {sortedInt}
                            WHEN decimalValue IS NOT NULL THEN {sortedDecimal}
                            WHEN dateValue IS NOT NULL THEN {sortedDate}
                            ELSE {sortedString}
                        END AS customPropVal,
                        cver.nodeId AS customPropNodeId")
                .From<ContentVersionDto>("cver")
                .InnerJoin<PropertyDataDto>("opdata")
                    .On<ContentVersionDto, PropertyDataDto>((version, pdata) => version.Id == pdata.VersionId, "cver", "opdata")
                .InnerJoin<PropertyTypeDto>("optype").On<PropertyDataDto, PropertyTypeDto>((pdata, ptype) => pdata.PropertyTypeId == ptype.Id, "opdata", "optype")
                .LeftJoin<LanguageDto>().On<PropertyDataDto, LanguageDto>((pdata, lang) => pdata.LanguageId == lang.Id, "opdata")
                .Where<ContentVersionDto>(x => x.Current, "cver") // always query on current (edit) values
                .Where<PropertyTypeDto>(x => x.Alias == ordering.OrderBy, "optype")
                .Where<PropertyDataDto, LanguageDto>((opdata, lang) => opdata.LanguageId == null || lang.IsoCode == ordering.Culture, "opdata");

            // merge arguments
            var argsList = sql.Arguments.ToList();
            var innerSqlString = ParameterHelper.ProcessParams(innerSql.SQL, innerSql.Arguments, argsList);

            // create the outer join complete sql fragment
            var outerJoinTempTable = $@"LEFT OUTER JOIN ({innerSqlString}) AS customPropData
                ON customPropData.customPropNodeId = {Constants.DatabaseSchema.Tables.Node}.id "; // trailing space is important!

            // insert this just above the first WHERE
            var newSql = InsertBefore(sql.SQL, "WHERE", outerJoinTempTable);

            // see notes in ApplyOrdering: the field MUST be selected + aliased
            newSql = InsertBefore(newSql, "FROM", ", customPropData.customPropVal AS ordering "); // trailing space is important!

            // create the new sql
            sql = Sql(newSql, argsList.ToArray());

            // and order by the custom field
            // this original code means that an ascending sort would first expose all NULL values, ie items without a value
            return "ordering";

            // note: adding an extra sorting criteria on
            // "(CASE WHEN customPropData.customPropVal IS NULL THEN 1 ELSE 0 END")
            // would ensure that items without a value always come last, both in ASC and DESC-ending sorts
        }

        public abstract IEnumerable<TEntity> GetPage(IQuery<TEntity>? query, long pageIndex, int pageSize, out long totalRecords, IQuery<TEntity>? filter, Ordering? ordering);

        public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        {
            var report = new Dictionary<int, ContentDataIntegrityReportEntry>();

            Sql<ISqlContext> sql = SqlContext.Sql()
                .Select<NodeDto>()
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            var nodesToRebuild = new Dictionary<int, List<NodeDto>>();
            var validNodes = new Dictionary<int, NodeDto>();
            var rootIds = new[] { Constants.System.Root, Constants.System.RecycleBinContent, Constants.System.RecycleBinMedia };
            var currentParentIds = new HashSet<int>(rootIds);
            HashSet<int> prevParentIds = currentParentIds;
            var lastLevel = -1;

            // use a forward cursor (query)
            foreach (NodeDto? node in Database.Query<NodeDto>(sql))
            {
                if (node.Level != lastLevel)
                {
                    // changing levels
                    prevParentIds = currentParentIds;
                    currentParentIds = null;
                    lastLevel = node.Level;
                }

                if (currentParentIds == null)
                {
                    // we're reset
                    currentParentIds = new HashSet<int>();
                }

                currentParentIds.Add(node.NodeId);

                // paths parts without the roots
                var pathParts = node.Path.Split(Constants.CharArrays.Comma).Where(x => !rootIds.Contains(int.Parse(x, CultureInfo.InvariantCulture))).ToArray();

                if (!prevParentIds.Contains(node.ParentId))
                {
                    // invalid, this will be because the level is wrong (which prob means path is wrong too)
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType.InvalidPathAndLevelByParentId));
                    AppendNodeToFix(nodesToRebuild, node);
                }
                else if (pathParts.Length == 0)
                {
                    // invalid path
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType.InvalidPathEmpty));
                    AppendNodeToFix(nodesToRebuild, node);
                }
                else if (pathParts.Length != node.Level)
                {
                    // invalid, either path or level is wrong
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType.InvalidPathLevelMismatch));
                    AppendNodeToFix(nodesToRebuild, node);
                }
                else if (pathParts[pathParts.Length - 1] != node.NodeId.ToString())
                {
                    // invalid path
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType.InvalidPathById));
                    AppendNodeToFix(nodesToRebuild, node);
                }
                else if (!rootIds.Contains(node.ParentId) && pathParts[pathParts.Length - 2] != node.ParentId.ToString())
                {
                    // invalid path
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType.InvalidPathByParentId));
                    AppendNodeToFix(nodesToRebuild, node);
                }
                else
                {
                    // it's valid!

                    // don't track unless we are configured to fix
                    if (options.FixIssues)
                    {
                        validNodes.Add(node.NodeId, node);
                    }
                }
            }

            var updated = new List<NodeDto>();

            if (options.FixIssues)
            {
                // iterate all valid nodes to see if these are parents for invalid nodes
                foreach (var (nodeId, node) in validNodes)
                {
                    if (!nodesToRebuild.TryGetValue(nodeId, out List<NodeDto>? invalidNodes))
                    {
                        continue;
                    }

                    // now we can try to rebuild the invalid paths.
                    foreach (NodeDto invalidNode in invalidNodes)
                    {
                        invalidNode.Level = (short)(node.Level + 1);
                        invalidNode.Path = node.Path + "," + invalidNode.NodeId;
                        updated.Add(invalidNode);
                    }
                }

                foreach (NodeDto node in updated)
                {
                    Database.Update(node);
                    if (report.TryGetValue(node.NodeId, out ContentDataIntegrityReportEntry? entry))
                    {
                        entry.Fixed = true;
                    }
                }
            }

            return new ContentDataIntegrityReport(report);
        }

        private static void AppendNodeToFix(IDictionary<int, List<NodeDto>> nodesToRebuild, NodeDto node)
        {
            if (nodesToRebuild.TryGetValue(node.ParentId, out List<NodeDto>? childIds))
            {
                childIds.Add(node);
            }
            else
            {
                nodesToRebuild[node.ParentId] = new List<NodeDto> { node };
            }
        }

        // here, filter can be null and ordering cannot
        protected IEnumerable<TEntity> GetPage<TDto>(
            IQuery<TEntity>? query,
            long pageIndex,
            int pageSize,
            out long totalRecords,
            Func<List<TDto>, IEnumerable<TEntity>> mapDtos,
            Sql<ISqlContext>? filter,
            Ordering? ordering)
        {
            if (ordering == null)
            {
                throw new ArgumentNullException(nameof(ordering));
            }

            // start with base query, and apply the supplied IQuery
            if (query == null)
            {
                query = Query<TEntity>();
            }

            Sql<ISqlContext> sql = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            sql = PreparePageSql(sql, filter, ordering);

            // get a page of DTOs and the total count
            Page<TDto>? pagedResult = Database.Page<TDto>(pageIndex + 1, pageSize, sql);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            // map the DTOs and return
            return mapDtos(pagedResult.Items);
        }

        protected IDictionary<int, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps)
            where T : class, IContentBase
        {
            var versions = new List<int>();
            foreach (TempContent<T> temp in temps)
            {
                versions.Add(temp.VersionId);
                if (temp.PublishedVersionId > 0)
                {
                    versions.Add(temp.PublishedVersionId);
                }
            }

            if (versions.Count == 0)
            {
                return new Dictionary<int, PropertyCollection>();
            }

            // TODO: This is a bugger of a query and I believe is the main issue with regards to SQL performance drain when querying content
            // which is done when rebuilding caches/indexes/etc... in bulk. We are using an "IN" query on umbracoPropertyData.VersionId
            // which then performs a Clustered Index Scan on PK_umbracoPropertyData which means it iterates the entire table which can be enormous!
            // especially if there are both a lot of content but worse if there is a lot of versions of that content.
            // So is it possible to return this property data without doing an index scan on PK_umbracoPropertyData and without iterating every row
            // in the table?

            // get all PropertyDataDto for all definitions / versions
            var allPropertyDataDtos = Database.FetchByGroups<PropertyDataDto, int>(versions, Constants.Sql.MaxParameterCount, batch =>
                SqlContext.Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .WhereIn<PropertyDataDto>(x => x.VersionId, batch))
                .ToList();

            // get PropertyDataDto distinct PropertyTypeDto
            var allPropertyTypeIds = allPropertyDataDtos.Select(x => x.PropertyTypeId).Distinct().ToList();
            IEnumerable<PropertyTypeDto> allPropertyTypeDtos = Database.FetchByGroups<PropertyTypeDto, int>(allPropertyTypeIds, Constants.Sql.MaxParameterCount, batch =>
                SqlContext.Sql()
                    .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto))
                    .From<PropertyTypeDto>()
                    .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
                    .WhereIn<PropertyTypeDto>(x => x.Id, batch));

            // index the types for perfs, and assign to PropertyDataDto
            var indexedPropertyTypeDtos = allPropertyTypeDtos.ToDictionary(x => x.Id, x => x);
            foreach (PropertyDataDto a in allPropertyDataDtos)
            {
                a.PropertyTypeDto = indexedPropertyTypeDtos[a.PropertyTypeId];
            }

            // now we have
            // - the definitions
            // - all property data dtos
            // - tag editors (Actually ... no we don't since i removed that code, but we don't need them anyways it seems)
            // and we need to build the proper property collections
            return GetPropertyCollections(temps, allPropertyDataDtos);
        }

        private IDictionary<int, PropertyCollection> GetPropertyCollections<T>(List<TempContent<T>> temps, IEnumerable<PropertyDataDto> allPropertyDataDtos)
            where T : class, IContentBase
        {
            var result = new Dictionary<int, PropertyCollection>();
            var compositionPropertiesIndex = new Dictionary<int, IPropertyType[]>();

            // index PropertyDataDto per versionId for perfs
            // merge edited and published dtos
            var indexedPropertyDataDtos = new Dictionary<int, List<PropertyDataDto>>();
            foreach (PropertyDataDto dto in allPropertyDataDtos)
            {
                var versionId = dto.VersionId;
                if (indexedPropertyDataDtos.TryGetValue(versionId, out List<PropertyDataDto>? list) == false)
                {
                    indexedPropertyDataDtos[versionId] = list = new List<PropertyDataDto>();
                }

                list.Add(dto);
            }

            foreach (TempContent<T> temp in temps)
            {
                // compositionProperties is the property types for the entire composition
                // use an index for perfs
                if (temp.ContentType is null)
                {
                    continue;
                }

                if (compositionPropertiesIndex.TryGetValue(temp.ContentType.Id, out IPropertyType[]? compositionProperties) == false)
                {
                    compositionPropertiesIndex[temp.ContentType.Id] = compositionProperties = temp.ContentType.CompositionPropertyTypes.ToArray();
                }

                // map the list of PropertyDataDto to a list of Property
                var propertyDataDtos = new List<PropertyDataDto>();
                if (indexedPropertyDataDtos.TryGetValue(temp.VersionId, out List<PropertyDataDto>? propertyDataDtos1))
                {
                    propertyDataDtos.AddRange(propertyDataDtos1);

                    // dirty corner case
                    if (temp.VersionId == temp.PublishedVersionId)
                    {
                        propertyDataDtos.AddRange(propertyDataDtos1.Select(x => x.Clone(-1)));
                    }
                }

                if (temp.VersionId != temp.PublishedVersionId && indexedPropertyDataDtos.TryGetValue(temp.PublishedVersionId, out List<PropertyDataDto>? propertyDataDtos2))
                {
                    propertyDataDtos.AddRange(propertyDataDtos2);
                }

                var properties = PropertyFactory.BuildEntities(compositionProperties, propertyDataDtos, temp.PublishedVersionId, LanguageRepository).ToList();

                if (result.ContainsKey(temp.VersionId))
                {
                    if (ContentRepositoryBase.ThrowOnWarning)
                    {
                        throw new InvalidOperationException($"The query returned multiple property sets for content {temp.Id}, {temp.ContentType.Name}");
                    }

                    Logger.LogWarning("The query returned multiple property sets for content {ContentId}, {ContentTypeName}", temp.Id, temp.ContentType.Name);
                }

                result[temp.VersionId] = new PropertyCollection(properties);
            }

            return result;
        }

        protected string InsertBefore(Sql<ISqlContext> s, string atToken, string insert)
            => InsertBefore(s.SQL, atToken, insert);

        protected string InsertBefore(string s, string atToken, string insert)
        {
            var pos = s.InvariantIndexOf(atToken);
            if (pos < 0)
            {
                throw new Exception($"Could not find token \"{atToken}\".");
            }

            return s.Insert(pos, insert);
        }

        protected Sql<ISqlContext> InsertJoins(Sql<ISqlContext> sql, Sql<ISqlContext> joins)
        {
            var joinsSql = joins.SQL;
            var args = sql.Arguments;

            // merge args if any
            if (joins.Arguments.Length > 0)
            {
                var argsList = args.ToList();
                joinsSql = ParameterHelper.ProcessParams(joinsSql, joins.Arguments, argsList);
                args = argsList.ToArray();
            }

            return Sql(InsertBefore(sql.SQL, "WHERE", joinsSql), args);
        }

        private string GetAliasedField(string field, Sql sql)
        {
            // get alias, if aliased
            //
            // regex looks for pattern "([\w+].[\w+]) AS ([\w+])" ie "(field) AS (alias)"
            // and, if found & a group's field matches the field name, returns the alias
            //
            // so... if query contains "[umbracoNode].[nodeId] AS [umbracoNode__nodeId]"
            // then GetAliased for "[umbracoNode].[nodeId]" returns "[umbracoNode__nodeId]"
            MatchCollection matches = SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
            Match? match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(field));
            return match == null ? field : match.Groups[2].Value;
        }

        protected string GetQuotedFieldName(string tableName, string fieldName)
        {
            return SqlContext.SqlSyntax.GetQuotedTableName(tableName) + "." + SqlContext.SqlSyntax.GetQuotedColumnName(fieldName);
        }

        #region UnitOfWork Notification

        /*
         * The reason why EntityRefreshNotification is published from the repository and not the service is because
         * the published state of the IContent must be "Publishing" when the event is raised for the cache to handle it correctly.
         * This state is changed half way through the repository method, meaning that if we publish the notification
         * after the state will be "Published" and the cache won't handle it correctly,
         * It wont call OnRepositoryRefreshed with the published flag set to true, the same is true for unpublishing
         * where it wont remove the entity from the nucache.
         * We can't publish the notification before calling Save method on the repository either,
         * because that method sets certain fields such as create date, update date, etc...
         */

        /// <summary>
        /// Publishes a notification, used to publish <see cref="EntityRefreshNotification{T}"/> for caching purposes.
        /// </summary>
        protected void OnUowRefreshedEntity(INotification notification) => _eventAggregator.Publish(notification);

        protected void OnUowRemovingEntity(IContentBase entity) => _eventAggregator.Publish(new ScopedEntityRemoveNotification(entity, new EventMessages()));
        #endregion

        #region Classes

        protected class TempContent
        {
            public TempContent(int id, int versionId, int publishedVersionId, IContentTypeComposition? contentType)
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
            public IContentTypeComposition? ContentType { get; set; }

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
            public TempContent(int id, int versionId, int publishedVersionId, IContentTypeComposition? contentType, T? content = null)
                : base(id, versionId, publishedVersionId, contentType)
            {
                Content = content;
            }

            /// <summary>
            /// Gets or sets the associated actual content.
            /// </summary>
            public T? Content { get; set; }
        }

        /// <summary>
        /// For Paging, repositories must support returning different query for the query type specified
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        protected abstract Sql<ISqlContext> GetBaseQuery(QueryType queryType);

        #endregion

        #region Utilities

        protected virtual string? EnsureUniqueNodeName(int parentId, string? nodeName, int id = 0)
        {
            SqlTemplate? template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.EnsureUniqueNodeName, tsql => tsql
                .Select<NodeDto>(x => Alias(x.NodeId, "id"), x => Alias(x.Text!, "name"))
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") && x.ParentId == SqlTemplate.Arg<int>("parentId")));

            Sql<ISqlContext> sql = template.Sql(NodeObjectTypeId, parentId);
            List<SimilarNodeName>? names = Database.Fetch<SimilarNodeName>(sql);

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        protected virtual int GetNewChildSortOrder(int parentId, int first)
        {
            SqlTemplate? template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.GetSortOrder, tsql => tsql
                .Select("MAX(sortOrder)")
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") && x.ParentId == SqlTemplate.Arg<int>("parentId")));

            Sql<ISqlContext> sql = template.Sql(NodeObjectTypeId, parentId);
            var sortOrder = Database.ExecuteScalar<int?>(sql);

            return sortOrder + 1 ?? first;
        }

        protected virtual NodeDto GetParentNodeDto(int parentId)
        {
            SqlTemplate? template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.GetParentNode, tsql => tsql
                .Select<NodeDto>()
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId")));

            Sql<ISqlContext> sql = template.Sql(parentId);
            NodeDto? nodeDto = Database.First<NodeDto>(sql);

            return nodeDto;
        }

        protected virtual int GetReservedId(Guid uniqueId)
        {
            SqlTemplate template = SqlContext.Templates.Get(Constants.SqlTemplates.VersionableRepository.GetReservedId, tsql => tsql
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("uniqueId") && x.NodeObjectType == Constants.ObjectTypes.IdReservation));

            Sql<ISqlContext> sql = template.Sql(new { uniqueId });
            var id = Database.ExecuteScalar<int?>(sql);

            return id ?? 0;
        }

        #endregion

        #region Recycle bin

        public abstract int RecycleBinId { get; }

        public virtual IEnumerable<TEntity>? GetRecycleBin()
        {
            return Get(Query<TEntity>().Where(entity => entity.Trashed));
        }

        #endregion

        protected void PersistRelations(TEntity entity)
        {
            // Get all references from our core built in DataEditors/Property Editors
            // Along with seeing if deverlopers want to collect additional references from the DataValueReferenceFactories collection
            var trackedRelations = new List<UmbracoEntityReference>();
            trackedRelations.AddRange(_dataValueReferenceFactories.GetAllReferences(entity.Properties, PropertyEditors));

            // First delete all auto-relations for this entity
            RelationRepository.DeleteByParent(entity.Id, Constants.Conventions.RelationTypes.AutomaticRelationTypes);

            if (trackedRelations.Count == 0)
            {
                return;
            }

            trackedRelations = trackedRelations.Distinct().ToList();
            var udiToGuids = trackedRelations.Select(x => x.Udi as GuidUdi)
                .ToDictionary(x => (Udi)x!, x => x!.Guid);

            // lookup in the DB all INT ids for the GUIDs and chuck into a dictionary
            var keyToIds = Database.Fetch<NodeIdKey>(Sql().Select<NodeDto>(x => x.NodeId, x => x.UniqueId).From<NodeDto>().WhereIn<NodeDto>(x => x.UniqueId, udiToGuids.Values))
                .ToDictionary(x => x.UniqueId, x => x.NodeId);

            var allRelationTypes = RelationTypeRepository.GetMany(Array.Empty<int>())?
                .ToDictionary(x => x.Alias, x => x);

            IEnumerable<ReadOnlyRelation> toSave = trackedRelations.Select(rel =>
                {
                    if (allRelationTypes is null || !allRelationTypes.TryGetValue(rel.RelationTypeAlias, out IRelationType? relationType))
                    {
                        throw new InvalidOperationException($"The relation type {rel.RelationTypeAlias} does not exist");
                    }

                    if (!udiToGuids.TryGetValue(rel.Udi, out Guid guid))
                    {
                        return null; // This shouldn't happen!
                    }

                    if (!keyToIds.TryGetValue(guid, out var id))
                    {
                        return null; // This shouldn't happen!
                    }

                    return new ReadOnlyRelation(entity.Id, id, relationType.Id);
                }).WhereNotNull();

            // Save bulk relations
            RelationRepository.SaveBulk(toSave);
        }

        /// <summary>
        /// Inserts property values for the content entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="publishedVersionId"></param>
        /// <param name="edited"></param>
        /// <param name="editedCultures"></param>
        /// <remarks>
        /// Used when creating a new entity
        /// </remarks>
        protected void InsertPropertyValues(TEntity entity, int publishedVersionId, out bool edited, out HashSet<string>? editedCultures)
        {
            // persist the property data
            IEnumerable<PropertyDataDto> propertyDataDtos = PropertyFactory.BuildDtos(entity.ContentType.Variations, entity.VersionId, publishedVersionId, entity.Properties, LanguageRepository, out edited, out editedCultures);
            foreach (PropertyDataDto? propertyDataDto in propertyDataDtos)
            {
                Database.Insert(propertyDataDto);
            }

            // TODO: we can speed this up: Use BulkInsert and then do one SELECT to re-retrieve the property data inserted with assigned IDs.
            // This is a perfect thing to benchmark with Benchmark.NET to compare perf between Nuget releases.
        }

        /// <summary>
        /// Used to atomically replace the property values for the entity version specified
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="versionId"></param>
        /// <param name="publishedVersionId"></param>
        /// <param name="edited"></param>
        /// <param name="editedCultures"></param>

        protected void ReplacePropertyValues(TEntity entity, int versionId, int publishedVersionId, out bool edited, out HashSet<string>? editedCultures)
        {
            // Replace the property data.
            // Lookup the data to update with a UPDLOCK (using ForUpdate()) this is because we need to be atomic
            // and handle DB concurrency. Doing a clear and then re-insert is prone to concurrency issues.
            Sql<ISqlContext> propDataSql = SqlContext.Sql().Select("*").From<PropertyDataDto>().Where<PropertyDataDto>(x => x.VersionId == versionId).ForUpdate();
            List<PropertyDataDto>? existingPropData = Database.Fetch<PropertyDataDto>(propDataSql);
            var propertyTypeToPropertyData = new Dictionary<(int propertyTypeId, int versionId, int? languageId, string? segment), PropertyDataDto>();
            var existingPropDataIds = new List<int>();
            foreach (PropertyDataDto? p in existingPropData)
            {
                existingPropDataIds.Add(p.Id);
                propertyTypeToPropertyData[(p.PropertyTypeId, p.VersionId, p.LanguageId, p.Segment)] = p;
            }

            IEnumerable<PropertyDataDto> propertyDataDtos = PropertyFactory.BuildDtos(entity.ContentType.Variations, entity.VersionId, publishedVersionId, entity.Properties, LanguageRepository, out edited, out editedCultures);

            foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
            {
                // Check if this already exists and update, else insert a new one
                if (propertyTypeToPropertyData.TryGetValue((propertyDataDto.PropertyTypeId, propertyDataDto.VersionId, propertyDataDto.LanguageId, propertyDataDto.Segment), out PropertyDataDto? propData))
                {
                    propertyDataDto.Id = propData.Id;
                    Database.Update(propertyDataDto);
                }
                else
                {
                    // TODO: we can speed this up: Use BulkInsert and then do one SELECT to re-retrieve the property data inserted with assigned IDs.
                    // This is a perfect thing to benchmark with Benchmark.NET to compare perf between Nuget releases.
                    Database.Insert(propertyDataDto);
                }

                // track which ones have been processed
                existingPropDataIds.Remove(propertyDataDto.Id);
            }

            // For any remaining that haven't been processed they need to be deleted
            if (existingPropDataIds.Count > 0)
            {
                Database.Execute(SqlContext.Sql().Delete<PropertyDataDto>().WhereIn<PropertyDataDto>(x => x.Id, existingPropDataIds));
            }
        }

        private class NodeIdKey
        {
            [Column("id")]
            public int NodeId { get; set; }

            [Column("uniqueId")]
            public Guid UniqueId { get; set; }
        }
    }
}
