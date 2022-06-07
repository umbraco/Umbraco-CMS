using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the EntityRepository used to query entity objects.
    /// </summary>
    /// <remarks>
    /// <para>Limited to objects that have a corresponding node (in umbracoNode table).</para>
    /// <para>Returns <see cref="IEntitySlim"/> objects, i.e. lightweight representation of entities.</para>
    /// </remarks>
    internal class EntityRepository : RepositoryBase, IEntityRepositoryExtended, IAsyncEntityRepository
    {
        public EntityRepository(IScopeAccessor scopeAccessor, AppCaches appCaches)
            : base(scopeAccessor, appCaches)
        {
        }

        #region Repository

        public IEnumerable<IEntitySlim> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
            IQuery<IUmbracoEntity>? filter, Ordering? ordering)
        {
            return GetPagedResultsByQuery(query, new[] { objectType }, pageIndex, pageSize, out totalRecords, filter, ordering);
        }
        public async Task<(IEnumerable<IEntitySlim> Entities, long TotalRecords)> GetPagedResultsByQueryAsync(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize,
           IQuery<IUmbracoEntity>? filter, Ordering? ordering)
        {
            return await GetPagedResultsByQueryAsync(query, new[] { objectType }, pageIndex, pageSize, filter, ordering);
        }

        // get a page of entities
        public IEnumerable<IEntitySlim> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid[] objectTypes, long pageIndex, int pageSize, out long totalRecords,
            IQuery<IUmbracoEntity>? filter, Ordering? ordering, Action<Sql<ISqlContext>>? sqlCustomization = null)
        {
            Sql<ISqlContext> sql = GetPagedResultsByQuerySql(query, objectTypes, filter, ref ordering, sqlCustomization);

            // for content we must query for ContentEntityDto entities to produce the correct culture variant entity names
            var pageIndexToFetch = pageIndex + 1;
            IEnumerable<BaseDto> dtos;
            var page = Database.Page<GenericContentEntityDto>(pageIndexToFetch, pageSize, sql);
            dtos = page.Items;
            totalRecords = page.TotalItems;

            var entities = dtos.Select(BuildEntity).ToArray();

            BuildVariants(entities.OfType<DocumentEntitySlim>());

            return entities;
        }

        // get a page of entities
        public async Task<(IEnumerable<IEntitySlim> Entities, long TotalRecords)> GetPagedResultsByQueryAsync(IQuery<IUmbracoEntity> query, Guid[] objectTypes, long pageIndex, int pageSize,
            IQuery<IUmbracoEntity>? filter, Ordering? ordering, Action<Sql<ISqlContext>>? sqlCustomization = null)
        {
            Sql<ISqlContext> sql = GetPagedResultsByQuerySql(query, objectTypes, filter, ref ordering, sqlCustomization);

            // for content we must query for ContentEntityDto entities to produce the correct culture variant entity names
            var pageIndexToFetch = pageIndex + 1;
            IEnumerable<BaseDto> dtos;
            var page = await Database.PageAsync<GenericContentEntityDto>(pageIndexToFetch, pageSize, sql);
            dtos = page.Items;
            var totalRecords = page.TotalItems;

            var entities = dtos.Select(BuildEntity).ToArray();

            BuildVariants(entities.OfType<DocumentEntitySlim>());

            return (entities,totalRecords);
        }

        private Sql<ISqlContext> GetPagedResultsByQuerySql(IQuery<IUmbracoEntity> query, Guid[] objectTypes, IQuery<IUmbracoEntity>? filter, ref Ordering? ordering, Action<Sql<ISqlContext>>? sqlCustomization)
        {
            var isContent = objectTypes.Any(objectType => objectType == Cms.Core.Constants.ObjectTypes.Document || objectType == Cms.Core.Constants.ObjectTypes.DocumentBlueprint);
            var isMedia = objectTypes.Any(objectType => objectType == Cms.Core.Constants.ObjectTypes.Media);
            var isMember = objectTypes.Any(objectType => objectType == Cms.Core.Constants.ObjectTypes.Member);

            Sql<ISqlContext> sql = GetBaseWhere(isContent, isMedia, isMember, false, s =>
            {
                sqlCustomization?.Invoke(s);

                if (filter != null)
                {
                    foreach (Tuple<string, object[]> filterClause in filter.GetWhereClauses())
                    {
                        s.Where(filterClause.Item1, filterClause.Item2);
                    }
                }
            }, objectTypes);

            ordering = ordering ?? Ordering.ByDefault();

            var translator = new SqlTranslator<IUmbracoEntity>(sql, query);
            sql = translator.Translate();
            sql = AddGroupBy(isContent, isMedia, isMember, sql, ordering.IsEmpty);

            if (!ordering.IsEmpty)
            {
                // apply ordering
                ApplyOrdering(ref sql, ordering);
            }

            // TODO: we should be able to do sql = sql.OrderBy(x => Alias(x.NodeId, "NodeId")); but we can't because the OrderBy extension don't support Alias currently
            // no matter what we always must have node id ordered at the end
            sql = ordering.Direction == Direction.Ascending ? sql.OrderBy("NodeId") : sql.OrderByDescending("NodeId");
            return sql;
        }

        public IEntitySlim? Get(Guid key)
        {
            var sql = GetBaseWhere(false, false, false, false, key);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(dto);
        }
        public async Task<IEntitySlim?> GetAsync(Guid key)
        {
            var sql = GetBaseWhere(false, false, false, false, key);
            var dto = await Database.FirstOrDefaultAsync<BaseDto>(sql);
            return dto == null ? null : BuildEntity(dto);
        }

        private IEntitySlim? GetEntity(Sql<ISqlContext> sql, bool isContent, bool isMedia, bool isMember)
        {
            // isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var cdtos = Database.Fetch<DocumentEntityDto>(sql);

                return cdtos.Count == 0 ? null : BuildVariants(BuildDocumentEntity(cdtos[0]));
            }

            var dto = isMedia
                    ? Database.FirstOrDefault<MediaEntityDto>(sql)
                    : Database.FirstOrDefault<BaseDto>(sql);

            if (dto == null) return null;

            var entity = BuildEntity(dto);

            return entity;
        }

        private async Task<IEntitySlim?> GetEntityAsync(Sql<ISqlContext> sql, bool isContent, bool isMedia, bool isMember)
        {
            // isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var cdtos = await Database.FetchAsync<DocumentEntityDto>(sql);

                return cdtos.Count == 0 ? null : BuildVariants(BuildDocumentEntity(cdtos[0]));
            }

            var dto = isMedia
                    ? await Database.FirstOrDefaultAsync<MediaEntityDto>(sql)
                    : await Database.FirstOrDefaultAsync<BaseDto>(sql);

            if (dto == null)
                return null;

            var entity = BuildEntity(dto);

            return entity;
        }

        public IEntitySlim? Get(Guid key, Guid objectTypeId)
        {
            var isContent = objectTypeId == Cms.Core.Constants.ObjectTypes.Document || objectTypeId == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectTypeId == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectTypeId, key);
            return GetEntity(sql, isContent, isMedia, isMember);
        }
        public async Task<IEntitySlim?> GetAsync(Guid key, Guid objectTypeId)
        {
            var isContent = objectTypeId == Cms.Core.Constants.ObjectTypes.Document || objectTypeId == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectTypeId == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectTypeId, key);
            return await GetEntityAsync(sql, isContent, isMedia, isMember);
        }

        public IEntitySlim? Get(int id)
        {
            var sql = GetBaseWhere(false, false, false, false, id);
            var dto = Database.FirstOrDefault<BaseDto>(sql);
            return dto == null ? null : BuildEntity(dto);
        }
        public async Task<IEntitySlim?> GetAsync(int id)
        {
            var sql = GetBaseWhere(false, false, false, false, id);
            var dto = await Database.FirstOrDefaultAsync<BaseDto>(sql);
            return dto == null ? null : BuildEntity(dto);
        }

        public IEntitySlim? Get(int id, Guid objectTypeId)
        {
            var isContent = objectTypeId == Cms.Core.Constants.ObjectTypes.Document || objectTypeId == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectTypeId == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectTypeId, id);
            return GetEntity(sql, isContent, isMedia, isMember);
        }

        public async Task<IEntitySlim?> GetAsync(int id, Guid objectTypeId)
        {
            var isContent = objectTypeId == Cms.Core.Constants.ObjectTypes.Document || objectTypeId == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectTypeId == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectTypeId == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectTypeId, id);
            return await GetEntityAsync(sql, isContent, isMedia, isMember);
        }

        public IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids)
        {
            return ids.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAll(objectType);
        }

        public async Task<IEnumerable<IEntitySlim>> GetAllAsync(Guid objectType, params int[] ids)
        {
            return ids.Length > 0
                ? await PerformGetAllAsync(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : await PerformGetAllAsync(objectType);
        }

        public IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys)
        {
            return keys.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAll(objectType);
        }

        public async Task<IEnumerable<IEntitySlim>> GetAllAsync(Guid objectType, params Guid[] keys)
        {
            return keys.Length > 0
                ? PerformGetAll(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAll(objectType);
        }

        private IEnumerable<IEntitySlim> GetEntities(Sql<ISqlContext> sql, bool isContent, bool isMedia, bool isMember)
        {
            // isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var cdtos = Database.Fetch<DocumentEntityDto>(sql);

                return cdtos.Count == 0
                    ? Enumerable.Empty<IEntitySlim>()
                    : BuildVariants(cdtos.Select(BuildDocumentEntity)).ToList();
            }

            var dtos = isMedia
                    ? (IEnumerable<BaseDto>)Database.Fetch<MediaEntityDto>(sql)
                    : Database.Fetch<BaseDto>(sql);

            var entities = dtos.Select(BuildEntity).ToArray();

            return entities;
        }
        private async Task<IEnumerable<IEntitySlim>> GetEntitiesAsync(Sql<ISqlContext> sql, bool isContent, bool isMedia, bool isMember)
        {
            // isContent is going to return a 1:M result now with the variants so we need to do different things
            if (isContent)
            {
                var cdtos = await Database.FetchAsync<DocumentEntityDto>(sql);

                return cdtos.Count == 0
                    ? Enumerable.Empty<IEntitySlim>()
                    : BuildVariants(cdtos.Select(BuildDocumentEntity)).ToList();
            }

            var dtos = isMedia
                    ? (IEnumerable<BaseDto>)Database.Fetch<MediaEntityDto>(sql)
                    : await Database.FetchAsync<BaseDto>(sql);

            var entities = dtos.Select(BuildEntity).ToArray();

            return entities;
        }
        private IEnumerable<IEntitySlim> PerformGetAll(Guid objectType, Action<Sql<ISqlContext>>? filter = null)
        {
            var isContent = objectType == Cms.Core.Constants.ObjectTypes.Document || objectType == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectType == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectType, filter);
            return GetEntities(sql, isContent, isMedia, isMember);
        }

        private async Task<IEnumerable<IEntitySlim>> PerformGetAllAsync(Guid objectType, Action<Sql<ISqlContext>>? filter = null)
        {
            var isContent = objectType == Cms.Core.Constants.ObjectTypes.Document || objectType == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            var isMedia = objectType == Cms.Core.Constants.ObjectTypes.Media;
            var isMember = objectType == Cms.Core.Constants.ObjectTypes.Member;

            var sql = GetFullSqlForEntityType(isContent, isMedia, isMember, objectType, filter);
            return await GetEntitiesAsync(sql, isContent, isMedia, isMember);
        }

        public IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[]? ids)
        {
            return ids?.Any() ?? false
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : PerformGetAllPaths(objectType);
        }
        public async Task<IEnumerable<TreeEntityPath>> GetAllPathsAsync(Guid objectType, params int[]? ids)
        {
            return ids?.Any() ?? false
                ? await PerformGetAllPathsAsync(objectType, sql => sql.WhereIn<NodeDto>(x => x.NodeId, ids.Distinct()))
                : await PerformGetAllPathsAsync(objectType);
        }

        public IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys)
        {
            return keys.Any()
                ? PerformGetAllPaths(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : PerformGetAllPaths(objectType);
        }
        public async Task<IEnumerable<TreeEntityPath>> GetAllPathsAsync(Guid objectType, params Guid[] keys)
        {
            return keys.Any()
                ? await PerformGetAllPathsAsync(objectType, sql => sql.WhereIn<NodeDto>(x => x.UniqueId, keys.Distinct()))
                : await PerformGetAllPathsAsync(objectType);
        }

        private IEnumerable<TreeEntityPath> PerformGetAllPaths(Guid objectType, Action<Sql<ISqlContext>>? filter = null)
        {
            Sql<ISqlContext> sql = GetAllPathsSql(objectType, filter);
            return Database.Fetch<TreeEntityPath>(sql);
        }
        private async Task<IEnumerable<TreeEntityPath>> PerformGetAllPathsAsync(Guid objectType, Action<Sql<ISqlContext>>? filter = null)
        {
            Sql<ISqlContext> sql = GetAllPathsSql(objectType, filter);
            return await Database.FetchAsync<TreeEntityPath>(sql);
        }

        private Sql<ISqlContext> GetAllPathsSql(Guid objectType, Action<Sql<ISqlContext>>? filter)
        {
            // NodeId is named Id on TreeEntityPath = use an alias
            var sql = Sql().Select<NodeDto>(x => Alias(x.NodeId, nameof(TreeEntityPath.Id)), x => x.Path).From<NodeDto>().Where<NodeDto>(x => x.NodeObjectType == objectType);
            filter?.Invoke(sql);
            return sql;
        }

        public IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            Sql<ISqlContext> sql = GetByQuerySql(query);
            var dtos = Database.Fetch<BaseDto>(sql);
            return dtos.Select(BuildEntity).ToList();
        }
        public async Task<IEnumerable<IEntitySlim>> GetByQueryAsync(IQuery<IUmbracoEntity> query)
        {
            Sql<ISqlContext> sql = GetByQuerySql(query);
            var dtos = await Database.FetchAsync<BaseDto>(sql);
            return dtos.Select(BuildEntity).ToList();
        }

        private Sql<ISqlContext> GetByQuerySql(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBase(false, false, false, null);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate();
            sql = AddGroupBy(false, false, false, sql, true);
            return sql;
        }

        public IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType)
        {
            bool isContent, isMedia, isMember;
            Sql<ISqlContext> sql;
            GetByQuerySql(query, objectType, out isContent, out isMedia, out isMember, out sql);

            return GetEntities(sql, isContent, isMedia, isMember);
        }
        public async Task<IEnumerable<IEntitySlim>> GetByQueryAsync(IQuery<IUmbracoEntity> query, Guid objectType)
        {
            bool isContent, isMedia, isMember;
            Sql<ISqlContext> sql;
            GetByQuerySql(query, objectType, out isContent, out isMedia, out isMember, out sql);

            return await GetEntitiesAsync(sql, isContent, isMedia, isMember);
        }

        private void GetByQuerySql(IQuery<IUmbracoEntity> query, Guid objectType, out bool isContent, out bool isMedia, out bool isMember, out Sql<ISqlContext> sql)
        {
            isContent = objectType == Cms.Core.Constants.ObjectTypes.Document || objectType == Cms.Core.Constants.ObjectTypes.DocumentBlueprint;
            isMedia = objectType == Cms.Core.Constants.ObjectTypes.Media;
            isMember = objectType == Cms.Core.Constants.ObjectTypes.Member;
            sql = GetBaseWhere(isContent, isMedia, isMember, false, null, new[] { objectType });
            var translator = new SqlTranslator<IUmbracoEntity>(sql, query);
            sql = translator.Translate();
            sql = AddGroupBy(isContent, isMedia, isMember, sql, true);
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

        public int ReserveId(Guid key)
        {
            NodeDto node;
            Sql<ISqlContext> sql = ReservedIdExistsSql(key);

            node = Database.SingleOrDefault<NodeDto>(sql);
            if (node != null)
                throw new InvalidOperationException("An identifier has already been reserved for this Udi.");

            node = MapToReservedNodeDto(key);
            Database.Insert(node);

            return node.NodeId;
        }
        public async Task<int> ReserveIdAsync(Guid key)
        {
            NodeDto node;
            Sql<ISqlContext> sql = ReservedIdExistsSql(key);

            node = await Database.SingleOrDefaultAsync<NodeDto>(sql);
            if (node != null)
                throw new InvalidOperationException("An identifier has already been reserved for this Udi.");

            node = MapToReservedNodeDto(key);
            await Database.InsertAsync(node);

            return node.NodeId;
        }

        private static NodeDto MapToReservedNodeDto(Guid key) => new NodeDto
        {
            UniqueId = key,
            Text = "RESERVED.ID",
            NodeObjectType = Cms.Core.Constants.ObjectTypes.IdReservation,

            CreateDate = DateTime.Now,
            UserId = null,
            ParentId = -1,
            Level = 1,
            Path = "-1",
            SortOrder = 0,
            Trashed = false
        };
        private Sql<ISqlContext> ReservedIdExistsSql(Guid key) =>
                    SqlContext.Sql()
                        .Select<NodeDto>()
                        .From<NodeDto>()
                        .Where<NodeDto>(x => x.UniqueId == key && x.NodeObjectType == Cms.Core.Constants.ObjectTypes.IdReservation);

        public bool Exists(Guid key)
        {
            Sql<ISqlContext> sql = ExistsSql(key);
            return Database.ExecuteScalar<int>(sql) > 0;
        }
        public async Task<bool> ExistsAsync(Guid key)
        {
            Sql<ISqlContext> sql = ExistsSql(key);
            return await Database.ExecuteScalarAsync<int>(sql) > 0;
        }

        private Sql<ISqlContext> ExistsSql(Guid key) => Sql().SelectCount().From<NodeDto>().Where<NodeDto>(x => x.UniqueId == key);

        public bool Exists(int id)
        {
            Sql<ISqlContext> sql = ExistsSql(id);
            return Database.ExecuteScalar<int>(sql) > 0;
        }
        public async Task<bool> ExistsAsync(int id)
        {
            Sql<ISqlContext> sql = ExistsSql(id);
            return await Database.ExecuteScalarAsync<int>(sql) > 0;
        }

        private Sql<ISqlContext> ExistsSql(int id) => Sql().SelectCount().From<NodeDto>().Where<NodeDto>(x => x.NodeId == id);
        private DocumentEntitySlim BuildVariants(DocumentEntitySlim entity)
            => BuildVariants(new[] { entity }).First();

        private IEnumerable<DocumentEntitySlim> BuildVariants(IEnumerable<DocumentEntitySlim> entities)
        {
            List<DocumentEntitySlim>? v = null;
            var entitiesList = entities.ToList();
            foreach (var e in entitiesList)
            {
                if (e.Variations.VariesByCulture())
                    (v ?? (v = new List<DocumentEntitySlim>())).Add(e);
            }

            if (v == null) return entitiesList;

            // fetch all variant info dtos
            var dtos = Database.FetchByGroups<VariantInfoDto, int>(v.Select(x => x.Id), Constants.Sql.MaxParameterCount, GetVariantInfos);

            // group by node id (each group contains all languages)
            var xdtos = dtos.GroupBy(x => x.NodeId).ToDictionary(x => x.Key, x => x);

            foreach (var e in v)
            {
                // since we're only iterating on entities that vary, we must have something
                var edtos = xdtos[e.Id];

                e.CultureNames = edtos.Where(x => x.CultureAvailable).ToDictionary(x => x.IsoCode, x => x.Name);
                e.PublishedCultures = edtos.Where(x => x.CulturePublished).Select(x => x.IsoCode);
                e.EditedCultures = edtos.Where(x => x.CultureAvailable && x.CultureEdited).Select(x => x.IsoCode);
            }

            return entitiesList;
        }

        #endregion

        #region Sql

        protected Sql<ISqlContext> GetVariantInfos(IEnumerable<int> ids)
        {
            return Sql()
                .Select<NodeDto>(x => x.NodeId)
                .AndSelect<LanguageDto>(x => x.IsoCode)
                .AndSelect<DocumentDto>("doc", x => Alias(x.Published, "DocumentPublished"), x => Alias(x.Edited, "DocumentEdited"))
                .AndSelect<DocumentCultureVariationDto>("dcv",
                    x => Alias(x.Available, "CultureAvailable"), x => Alias(x.Published, "CulturePublished"), x => Alias(x.Edited, "CultureEdited"),
                    x => Alias(x.Name, "Name"))

                // from node x language
                .From<NodeDto>()
                .CrossJoin<LanguageDto>()

                // join to document - always exists - indicates global document published/edited status
                .InnerJoin<DocumentDto>("doc")
                    .On<NodeDto, DocumentDto>((node, doc) => node.NodeId == doc.NodeId, aliasRight: "doc")

                // left-join do document variation - matches cultures that are *available* + indicates when *edited*
                .LeftJoin<DocumentCultureVariationDto>("dcv")
                    .On<NodeDto, DocumentCultureVariationDto, LanguageDto>((node, dcv, lang) => node.NodeId == dcv.NodeId && lang.Id == dcv.LanguageId, aliasRight: "dcv")

                // for selected nodes
                .WhereIn<NodeDto>(x => x.NodeId, ids)
                .OrderBy<LanguageDto>(x => x.Id);
        }

        // gets the full sql for a given object type and a given unique id
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, bool isMember, Guid objectType, Guid uniqueId)
        {
            var sql = GetBaseWhere(isContent, isMedia, isMember, false, objectType, uniqueId);
            return AddGroupBy(isContent, isMedia, isMember, sql, true);
        }

        // gets the full sql for a given object type and a given node id
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, bool isMember, Guid objectType, int nodeId)
        {
            var sql = GetBaseWhere(isContent, isMedia, isMember, false, objectType, nodeId);
            return AddGroupBy(isContent, isMedia, isMember, sql, true);
        }

        // gets the full sql for a given object type, with a given filter
        protected Sql<ISqlContext> GetFullSqlForEntityType(bool isContent, bool isMedia, bool isMember, Guid objectType, Action<Sql<ISqlContext>>? filter)
        {
            var sql = GetBaseWhere(isContent, isMedia, isMember, false, filter, new[] { objectType });
            return AddGroupBy(isContent, isMedia, isMember, sql, true);
        }

        // gets the base SELECT + FROM [+ filter] sql
        // always from the 'current' content version
        protected Sql<ISqlContext> GetBase(bool isContent, bool isMedia, bool isMember, Action<Sql<ISqlContext>>? filter, bool isCount = false)
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

                if (isContent || isMedia || isMember)
                    sql
                        .AndSelect<ContentVersionDto>(x => Alias(x.Id, "versionId"), x=>x.VersionDate)
                        .AndSelect<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer, x => x.Variations);

                if (isContent)
                {
                    sql
                        .AndSelect<DocumentDto>(x => x.Published, x => x.Edited);
                }

                if (isMedia)
                {
                    sql
                        .AndSelect<MediaVersionDto>(x => Alias(x.Path, "MediaPath"));
                }
            }

            sql
                .From<NodeDto>();

            if (isContent || isMedia || isMember)
            {
                sql
                    .LeftJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .LeftJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .LeftJoin<ContentTypeDto>().On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId);
            }

            if (isContent)
            {
                sql
                    .LeftJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId);
            }

            if (isMedia)
            {
                sql
                    .LeftJoin<MediaVersionDto>().On<ContentVersionDto, MediaVersionDto>((left, right) => left.Id == right.Id);
            }

            //Any LeftJoin statements need to come last
            if (isCount == false)
            {
                sql
                    .LeftJoin<NodeDto>("child").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.ParentId, aliasRight: "child");
            }


            filter?.Invoke(sql);

            return sql;
        }

        // gets the base SELECT + FROM [+ filter] + WHERE sql
        // for a given object type, with a given filter
        protected Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isMember, bool isCount, Action<Sql<ISqlContext>>? filter, Guid[] objectTypes)
        {
            var sql = GetBase(isContent, isMedia, isMember, filter, isCount);
            if (objectTypes.Length > 0)
            {
                sql.WhereIn<NodeDto>(x => x.NodeObjectType, objectTypes);
            }
            return sql;
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given node id
        protected Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isMember, bool isCount, int id)
        {
            var sql = GetBase(isContent, isMedia, isMember, null, isCount)
                .Where<NodeDto>(x => x.NodeId == id);
            return AddGroupBy(isContent, isMedia, isMember, sql, true);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given unique id
        protected Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isMember, bool isCount, Guid uniqueId)
        {
            var sql = GetBase(isContent, isMedia, isMember, null, isCount)
                .Where<NodeDto>(x => x.UniqueId == uniqueId);
            return AddGroupBy(isContent, isMedia, isMember, sql, true);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given object type and node id
        protected Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isMember, bool isCount, Guid objectType, int nodeId)
        {
            return GetBase(isContent, isMedia, isMember, null, isCount)
                .Where<NodeDto>(x => x.NodeId == nodeId && x.NodeObjectType == objectType);
        }

        // gets the base SELECT + FROM + WHERE sql
        // for a given object type and unique id
        protected Sql<ISqlContext> GetBaseWhere(bool isContent, bool isMedia, bool isMember, bool isCount, Guid objectType, Guid uniqueId)
        {
            return GetBase(isContent, isMedia, isMember, null, isCount)
                .Where<NodeDto>(x => x.UniqueId == uniqueId && x.NodeObjectType == objectType);
        }

        // gets the GROUP BY / ORDER BY sql
        // required in order to count children
        protected Sql<ISqlContext> AddGroupBy(bool isContent, bool isMedia, bool isMember, Sql<ISqlContext> sql, bool defaultSort)
        {
            sql
                .GroupBy<NodeDto>(x => x.NodeId, x => x.Trashed, x => x.ParentId, x => x.UserId, x => x.Level, x => x.Path)
                .AndBy<NodeDto>(x => x.SortOrder, x => x.UniqueId, x => x.Text, x => x.NodeObjectType, x => x.CreateDate);

            if (isContent)
            {
                sql
                    .AndBy<DocumentDto>(x => x.Published, x => x.Edited);
            }

            if (isMedia)
            {
                sql
                    .AndBy<MediaVersionDto>(x => Alias(x.Path, "MediaPath"));
            }


            if (isContent || isMedia || isMember)
                sql
                    .AndBy<ContentVersionDto>(x => x.Id, x => x.VersionDate)
                    .AndBy<ContentTypeDto>(x => x.Alias, x => x.Icon, x => x.Thumbnail, x => x.IsContainer, x => x.Variations);

            if (defaultSort)
                sql.OrderBy<NodeDto>(x => x.SortOrder);

            return sql;
        }

        private void ApplyOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            if (ordering == null) throw new ArgumentNullException(nameof(ordering));

            // TODO: although the default ordering string works for name, it wont work for others without a table or an alias of some sort
            // As more things are attempted to be sorted we'll prob have to add more expressions here
            string orderBy;
            switch (ordering.OrderBy?.ToUpperInvariant())
            {
                case "PATH":
                    orderBy = SqlSyntax.GetQuotedColumn(NodeDto.TableName, "path");
                    break;

                default:
                    orderBy = ordering.OrderBy ?? string.Empty;
                    break;
            }

            if (ordering.Direction == Direction.Ascending)
                sql.OrderBy(orderBy);
            else
                sql.OrderByDescending(orderBy);
        }

        #endregion

        #region Classes

        /// <summary>
        /// The DTO used to fetch results for a generic content item which could be either a document, media or a member
        /// </summary>
        private class GenericContentEntityDto : DocumentEntityDto
        {
            public string? MediaPath { get; set; }
        }

        /// <summary>
        /// The DTO used to fetch results for a document item with its variation info
        /// </summary>
        private class DocumentEntityDto : BaseDto
        {
            public ContentVariation Variations { get; set; }

            public bool Published { get; set; }
            public bool Edited { get; set; }
        }

        /// <summary>
        /// The DTO used to fetch results for a media item with its media path info
        /// </summary>
        private class MediaEntityDto : BaseDto
        {
            public string? MediaPath { get; set; }
        }

        /// <summary>
        /// The DTO used to fetch results for a member item
        /// </summary>
        private class MemberEntityDto : BaseDto
        {
        }

        public class VariantInfoDto
        {
            public int NodeId { get; set; }
            public string IsoCode { get; set; } = null!;
            public string Name { get; set; } = null!;
            public bool DocumentPublished { get; set; }
            public bool DocumentEdited { get; set; }

            public bool CultureAvailable { get; set; }
            public bool CulturePublished { get; set; }
            public bool CultureEdited { get; set; }
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
            public string Path { get; set; } = null!;
            public int SortOrder { get; set; }
            public Guid UniqueId { get; set; }
            public string? Text { get; set; }
            public Guid NodeObjectType { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime VersionDate { get; set; }
            public int Children { get; set; }
            public int VersionId { get; set; }
            public string Alias { get; set; } = null!;
            public string? Icon { get; set; }
            public string? Thumbnail { get; set; }
            public bool IsContainer { get; set; }

            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore UnusedMember.Local
        }
        #endregion

        #region Factory

        private EntitySlim BuildEntity(BaseDto dto)
        {
            if (dto.NodeObjectType == Cms.Core.Constants.ObjectTypes.Document)
                return BuildDocumentEntity(dto);
            if (dto.NodeObjectType == Cms.Core.Constants.ObjectTypes.Media)
                return BuildMediaEntity(dto);
            if (dto.NodeObjectType == Cms.Core.Constants.ObjectTypes.Member)
                return BuildMemberEntity(dto);

            // EntitySlim does not track changes
            var entity = new EntitySlim();
            BuildEntity(entity, dto);
            return entity;
        }

        private static void BuildEntity(EntitySlim entity, BaseDto dto)
        {
            entity.Trashed = dto.Trashed;
            entity.CreateDate = dto.CreateDate;
            entity.UpdateDate = dto.VersionDate;
            entity.CreatorId = dto.UserId ?? Cms.Core.Constants.Security.UnknownUserId;
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

        private MediaEntitySlim BuildMediaEntity(BaseDto dto)
        {
            // EntitySlim does not track changes
            var entity = new MediaEntitySlim();
            BuildContentEntity(entity, dto);

            // fill in the media info
            if (dto is MediaEntityDto mediaEntityDto)
            {
                entity.MediaPath = mediaEntityDto.MediaPath;
            }
            else if (dto is GenericContentEntityDto genericContentEntityDto)
            {
                entity.MediaPath = genericContentEntityDto.MediaPath;
            }

            return entity;
        }

        private DocumentEntitySlim BuildDocumentEntity(BaseDto dto)
        {
            // EntitySlim does not track changes
            var entity = new DocumentEntitySlim();
            BuildContentEntity(entity, dto);

            if (dto is DocumentEntityDto contentDto)
            {
                // fill in the invariant info
                entity.Edited = contentDto.Edited;
                entity.Published = contentDto.Published;
                entity.Variations = contentDto.Variations;
            }

            return entity;
        }

        private MemberEntitySlim BuildMemberEntity(BaseDto dto)
        {
            // EntitySlim does not track changes
            var entity = new MemberEntitySlim();
            BuildEntity(entity, dto);

            entity.ContentTypeAlias = dto.Alias;
            entity.ContentTypeIcon = dto.Icon;
            entity.ContentTypeThumbnail = dto.Thumbnail;

            return entity;
        }

        #endregion
    }
}
