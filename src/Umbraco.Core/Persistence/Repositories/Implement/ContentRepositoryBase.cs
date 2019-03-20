﻿using System;
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
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
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
        protected ContentRepositoryBase(IScopeAccessor scopeAccessor, AppCaches cache, ILanguageRepository languageRepository, ILogger logger)
            : base(scopeAccessor, cache, logger)
        {
            LanguageRepository = languageRepository;
        }

        protected abstract TRepository This { get; }

        protected ILanguageRepository LanguageRepository { get; }

        protected PropertyEditorCollection PropertyEditors => Current.PropertyEditors; // TODO: inject

        #region Versions

        // gets a specific version
        public abstract TEntity GetVersion(int versionId);

        // gets all versions, current first
        public abstract IEnumerable<TEntity> GetAllVersions(int nodeId);

        // gets all versions, current first
        public virtual IEnumerable<TEntity> GetAllVersionsSlim(int nodeId, int skip, int take)
            => GetAllVersions(nodeId).Skip(skip).Take(take);

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
            // TODO: test object node type?

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
            // TODO: test object node type?

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
                if (tagConfiguration == null) continue; // not a tags property

                if (property.PropertyType.VariesByCulture())
                {
                    var tags = new List<ITag>();
                    foreach (var pvalue in property.Values)
                    {
                        var tagsValue = property.GetTagsValue(pvalue.Culture);
                        var languageId = LanguageRepository.GetIdByIsoCode(pvalue.Culture);
                        var cultureTags = tagsValue.Select(x => new Tag { Group = tagConfiguration.Group, Text = x, LanguageId = languageId });
                        tags.AddRange(cultureTags);
                    }
                    tagRepo.Assign(entity.Id, property.PropertyTypeId, tags);
                }
                else
                {
                    var tagsValue = property.GetTagsValue(); // strings
                    var tags = tagsValue.Select(x => new Tag { Group = tagConfiguration.Group, Text = x });
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

        private Sql<ISqlContext> PreparePageSql(Sql<ISqlContext> sql, Sql<ISqlContext> filterSql, Ordering ordering)
        {
            // non-filtering, non-ordering = nothing to do
            if (filterSql == null && ordering.IsEmpty) return sql;

            // preserve original
            var psql = new Sql<ISqlContext>(sql.SqlContext, sql.SQL, sql.Arguments);

            // apply filter
            if (filterSql != null)
                psql.Append(filterSql);

            // non-sorting, we're done
            if (ordering.IsEmpty)
                return psql;

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
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            if (ordering == null) throw new ArgumentNullException(nameof(ordering));

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
                sql.OrderBy(orderBy);
            else
                sql.OrderByDescending(orderBy);
        }

        protected virtual string ApplySystemOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
        {
            // id is invariant
            if (ordering.OrderBy.InvariantEquals("id"))
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.NodeId), sql);

            // sort order is invariant
            if (ordering.OrderBy.InvariantEquals("sortOrder"))
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.SortOrder), sql);

            // path is invariant
            if (ordering.OrderBy.InvariantEquals("path"))
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.Path), sql);

            // note: 'owner' is the user who created the item as a whole,
            //       we don't have an 'owner' per culture (should we?)
            if (ordering.OrderBy.InvariantEquals("owner"))
            {
                var joins = Sql()
                    .InnerJoin<UserDto>("ownerUser").On<NodeDto, UserDto>((node, user) => node.UserId == user.Id, aliasRight: "ownerUser");

                // see notes in ApplyOrdering: the field MUST be selected + aliased
                sql = Sql(InsertBefore(sql, "FROM", ", " + SqlSyntax.GetFieldName<UserDto>(x => x.UserName, "ownerUser") + " AS ordering "), sql.Arguments);

                sql = InsertJoins(sql, joins);

                return "ordering";
            }

            // note: each version culture variation has a date too,
            //       maybe we would want to use it instead?
            if (ordering.OrderBy.InvariantEquals("versionDate") || ordering.OrderBy.InvariantEquals("updateDate"))
                return GetAliasedField(SqlSyntax.GetFieldName<ContentVersionDto>(x => x.VersionDate), sql);

            // create date is invariant (we don't keep each culture's creation date)
            if (ordering.OrderBy.InvariantEquals("createDate"))
                return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.CreateDate), sql);

            // name is variant
            if (ordering.OrderBy.InvariantEquals("name"))
            {
                // no culture = can only work on the invariant name
                // see notes in ApplyOrdering: the field MUST be aliased
                if (ordering.Culture.IsNullOrWhiteSpace())
                    return GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.Text), sql);

                // "variantName" alias is defined in DocumentRepository.GetBaseQuery
                // TODO: what if it is NOT a document but a ... media or whatever?
                // previously, we inserted the join+select *here* so we were sure to have it,
                // but now that's not the case anymore!
                return "variantName";
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
            var innerSql = Sql().Select($@"CASE
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

        public abstract IEnumerable<TEntity> GetPage(IQuery<TEntity> query,
            long pageIndex, int pageSize, out long totalRecords,
            IQuery<TEntity> filter,
            Ordering ordering);

        // here, filter can be null and ordering cannot
        protected IEnumerable<TEntity> GetPage<TDto>(IQuery<TEntity> query,
            long pageIndex, int pageSize, out long totalRecords,
            Func<List<TDto>, IEnumerable<TEntity>> mapDtos,
            Sql<ISqlContext> filter,
            Ordering ordering)
        {
            if (ordering == null) throw new ArgumentNullException(nameof(ordering));

            // start with base query, and apply the supplied IQuery
            if (query == null) query = Query<TEntity>();
            var sql = new SqlTranslator<TEntity>(GetBaseQuery(QueryType.Many), query).Translate();

            // sort and filter
            sql = PreparePageSql(sql, filter, ordering);

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
            // - the definitions
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

        protected string InsertBefore(Sql<ISqlContext> s, string atToken, string insert)
            => InsertBefore(s.SQL, atToken, insert);

        protected string InsertBefore(string s, string atToken, string insert)
        {
            var pos = s.InvariantIndexOf(atToken);
            if (pos < 0) throw new Exception($"Could not find token \"{atToken}\".");
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

            var matches = SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
            var match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(field));
            return match == null ? field : match.Groups[2].Value;
        }

        protected string GetQuotedFieldName(string tableName, string fieldName)
        {
            return SqlContext.SqlSyntax.GetQuotedTableName(tableName) + "." + SqlContext.SqlSyntax.GetQuotedColumnName(fieldName);
        }

        #region UnitOfWork Events

        // TODO: The reason these events are in the repository is for legacy, the events should exist at the service
        // level now since we can fire these events within the transaction... so move the events to service level

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

        /// <summary>
        /// For Paging, repositories must support returning different query for the query type specified
        /// </summary>
        /// <param name="queryType"></param>
        /// <returns></returns>
        protected abstract Sql<ISqlContext> GetBaseQuery(QueryType queryType);

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
