using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NPoco;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMember"/>
    /// </summary>
    internal class MemberRepository : VersionableRepositoryBase<int, IMember, MemberRepository>, IMemberRepository
    {
        private readonly IMemberTypeRepository _memberTypeRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMemberGroupRepository _memberGroupRepository;

        public MemberRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, IMemberTypeRepository memberTypeRepository, IMemberGroupRepository memberGroupRepository, ITagRepository tagRepository)
            : base(work, cache, logger)
        {
            _memberTypeRepository = memberTypeRepository ?? throw new ArgumentNullException(nameof(memberTypeRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _memberGroupRepository = memberGroupRepository;
        }

        protected override MemberRepository This => this;

        #region Repository Base

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Member;

        protected override IMember PerformGet(int id)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.NodeId == id)
                .SelectTop(1);

            var dto = Database.Fetch<MemberDto>(sql).FirstOrDefault();
            return dto == null
                ? null
                : MapDtoToContent(dto);
        }

        protected override IEnumerable<IMember> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(QueryType.Many);

            if (ids.Any())
                sql.WhereIn<NodeDto>(x => x.NodeId, ids);

            return MapDtosToContent(Database.Fetch<MemberDto>(sql));
        }

        protected override IEnumerable<IMember> PerformGetByQuery(IQuery<IMember> query)
        {
            var baseQuery = GetBaseQuery(false);

            // fixme why is this different from content/media?!
            //check if the query is based on properties or not

            var wheres = query.GetWhereClauses();
            //this is a pretty rudimentary check but wil work, we just need to know if this query requires property
            // level queries
            if (wheres.Any(x => x.Item1.Contains("cmsPropertyType")))
            {
                var sqlWithProps = GetNodeIdQueryWithPropertyData();
                var translator = new SqlTranslator<IMember>(sqlWithProps, query);
                var sql = translator.Translate();

                baseQuery.Append("WHERE umbracoNode.id IN (" + sql.SQL + ")", sql.Arguments)
                    .OrderBy<NodeDto>(x => x.SortOrder);

                return MapDtosToContent(Database.Fetch<MemberDto>(baseQuery));
            }
            else
            {
                var translator = new SqlTranslator<IMember>(baseQuery, query);
                var sql = translator.Translate()
                    .OrderBy<NodeDto>(x => x.SortOrder);

                return MapDtosToContent(Database.Fetch<MemberDto>(sql));
            }

        }

        protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType)
        {
            return GetBaseQuery(queryType, true);
        }

        protected virtual Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current)
        {
            var sql = SqlContext.Sql();

            switch (queryType) // FIXME pretend we still need these queries for now
            {
                case QueryType.Count:
                    sql = sql.SelectCount();
                    break;
                case QueryType.Ids:
                    sql = sql.Select<MemberDto>(x => x.NodeId);
                    break;
                case QueryType.Single:
                case QueryType.Many:
                    sql = sql.Select<MemberDto>(r =>
                        r.Select(x => x.ContentVersionDto)
                         .Select(x => x.ContentDto, r1 =>
                                r1.Select(x => x.NodeDto)));
                    break;
            }

            sql
                .From<MemberDto>()
                .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, MemberDto>(left => left.NodeId, right => right.NodeId)

                // joining the type so we can do a query against the member type - not sure if this adds much overhead or not?
                // the execution plan says it doesn't so we'll go with that and in that case, it might be worth joining the content
                // types by default on the document and media repo's so we can query by content type there too.
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId);

            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            if (current)
                sql.Where<ContentVersionDto>(x => x.Current); // always get the current version

            return sql;
        }

        // fixme - move that one up to Versionable! or better: kill it!
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);
        }

        protected override string GetBaseWhereClause() // fixme - can we kill / refactor this?
        {
            return "umbracoNode.id = @Id";
        }

        // fixme wtf?
        protected Sql<ISqlContext> GetNodeIdQueryWithPropertyData()
        {
            return Sql()
                .Select("DISTINCT(umbracoNode.id)")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND " + Constants.DatabaseSchema.Tables.PropertyData + ".versionId = cmsContentVersion.VersionId")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM cmsTask WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE nodeId = @Id",
                               "DELETE FROM cmsMember2MemberGroup WHERE Member = @Id",
                               "DELETE FROM cmsMember WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        #endregion

        #region Versions

        public override IEnumerable<IMember> GetAllVersions(int nodeId)
        {
            var sql = GetBaseQuery(QueryType.Many, false)
                .Where<NodeDto>(x => x.NodeId == nodeId)
                .OrderByDescending<ContentVersionDto>(x => x.Current)
                .AndByDescending<ContentVersionDto>(x => x.VersionDate);

            return MapDtosToContent(Database.Fetch<MemberDto>(sql), true);
        }

        public override IMember GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<ContentVersionDto>(x => x.VersionId == versionId);

            var dto = Database.Fetch<MemberDto>(sql).FirstOrDefault();
            return dto == null ? null : MapDtoToContent(dto);
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            // raise event first else potential FK issues
            OnUowRemovingVersion(new UnitOfWorkVersionEventArgs(UnitOfWork, id, versionId));

            Database.Delete<PropertyDataDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Persist

        protected override void PersistNewItem(IMember entity)
        {
            ((Member) entity).AddingEntity();

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // create the dto
            var dto = MemberFactory.BuildDto(entity);

            // derive path and level from parent
            var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetParentNode", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId"))
            );
            var parent = Database.Fetch<NodeDto>(template.Sql(entity.ParentId)).First();
            var level = parent.Level + 1;

            // get sort order
            var sortOrder = GetNewChildSortOrder(entity.ParentId, 0);

            // persist the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = Convert.ToInt16(level);
            nodeDto.SortOrder = sortOrder;

            // see if there's a reserved identifier for this unique id
            // and then either update or insert the node dto
            template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetReservedId", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("uniqueId") && x.NodeObjectType == Constants.ObjectTypes.IdReservation)
            );
            var id = Database.ExecuteScalar<int>(template.Sql(nodeDto.UniqueId)); // fixme can we mix named & non-named?
            if (id > 0)
            {
                nodeDto.NodeId = id;
                nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
                nodeDto.ValidatePathWithException();
                Database.Update(nodeDto);
            }
            else
            {
                Database.Insert(nodeDto);

                // update path, now that we have an id
                nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
                nodeDto.ValidatePathWithException();
                Database.Update(nodeDto);
            }

            // update entity
            entity.Id = nodeDto.NodeId;
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            // persist the content dto
            var contentDto = dto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            // persist the content version dto
            // assumes a new version id and version date (modified date) has been set
            var contentVersionDto = dto.ContentVersionDto; // fixme version id etc?
            contentVersionDto.NodeId = nodeDto.NodeId;
            contentVersionDto.Current = true;
            Database.Insert(contentVersionDto);

            // persist the member dto
            dto.NodeId = nodeDto.NodeId; // fixme version id etc?

            // if the password is empty, generate one with the special prefix
            // this will hash the guid with a salt so should be nicely random
            if (entity.RawPasswordValue.IsNullOrWhiteSpace())
            {
                var aspHasher = new PasswordHasher();
                dto.Password = Constants.Security.EmptyPasswordPrefix + aspHasher.HashPassword(Guid.NewGuid().ToString("N"));
                entity.RawPasswordValue = dto.Password;
            }

            Database.Insert(dto);

            // persist the property data
            var propertyDataDtos = PropertyFactory.BuildDtos(entity.Id, entity.Version, entity.Properties).ToArray();
            foreach (var propertyDataDto in propertyDataDtos)
                Database.Insert(propertyDataDto);

            // assign ids to properties, using propertyTypeId as a key
            var xids = propertyDataDtos.ToDictionary(x => x.PropertyTypeId, x => x.Id);
            foreach (var property in entity.Properties)
                property.Id = xids[property.PropertyTypeId];

            UpdateEntityTags(entity, _tagRepository);

            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMember entity)
        {
            //Updates Modified date
            ((Member) entity).UpdatingEntity();

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // if parent has changed, get path, level and sort order
            if (entity.IsPropertyDirty("ParentId"))
            {
                var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetParentNode", tsql =>
                    tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId"))
                );
                var parent = Database.Fetch<NodeDto>(template.Sql(entity.ParentId)).First();

                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
            }

            // create the dto
            var dto = MemberFactory.BuildDto(entity);

            // update the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            Database.Update(nodeDto);

            // update the content dto - only if the content type has changed
            var origContentDto = Database.Fetch<ContentDto>(SqlContext.Sql().Select<ContentDto>().From<ContentDto>().Where<ContentDto>(x => x.NodeId == entity.Id)).FirstOrDefault();
            if (origContentDto.ContentTypeId != entity.ContentTypeId)
            {
                dto.ContentDto.Id = origContentDto.Id; // fixme - annoying, needed?
                Database.Update(dto.ContentDto);
            }

            // insert or update the content version dtos
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { /*Version =*/ entity.Version });
            dto.ContentVersionDto.Id = contentVerDto.Id;
            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
            Database.Update(dto.ContentVersionDto);

            // update the member dto
            // but only the changed columns, 'cos we cannot update password if empty
            var changedCols = new List<string>();

            if (entity.IsPropertyDirty("Email"))
                changedCols.Add("Email");

            if (entity.IsPropertyDirty("Username"))
                changedCols.Add("LoginName");

            // do NOT update the password if it has not changed or if it is null or empty
            if (entity.IsPropertyDirty("RawPasswordValue") && !string.IsNullOrWhiteSpace(entity.RawPasswordValue))
                changedCols.Add("Password");

            if (changedCols.Count > 0)
                Database.Update(dto, changedCols);

            // update the property data
            var propertyDataDtos = PropertyFactory.BuildDtos(entity.Id, entity.Version, entity.Properties).ToArray();
            foreach (var propertyDataDto in propertyDataDtos)
            {
                if (propertyDataDto.Id > 0)
                    Database.Update(propertyDataDto);
                else
                    Database.Insert(propertyDataDto);
            }

            // assign ids to properties, using propertyTypeId as a key
            var xids = propertyDataDtos.ToDictionary(x => x.PropertyTypeId, x => x.Id);
            foreach (var property in entity.Properties)
                property.Id = xids[property.PropertyTypeId];

            UpdateEntityTags(entity, _tagRepository);

            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IMember entity)
        {
            // raise event first else potential FK issues
            OnUowRemovingEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));
            base.PersistDeletedItem(entity);
        }

        #endregion

        public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            //get the group id
            var grpQry = Query<IMemberGroup>().Where(group => group.Name.Equals(roleName));
            var memberGroup = _memberGroupRepository.GetByQuery(grpQry).FirstOrDefault();
            if (memberGroup == null) return Enumerable.Empty<IMember>();

            // get the members by username
            var query = Query<IMember>();
            switch (matchType)
            {
                case StringPropertyMatchType.Exact:
                    query.Where(member => member.Username.Equals(usernameToMatch));
                    break;
                case StringPropertyMatchType.Contains:
                    query.Where(member => member.Username.Contains(usernameToMatch));
                    break;
                case StringPropertyMatchType.StartsWith:
                    query.Where(member => member.Username.StartsWith(usernameToMatch));
                    break;
                case StringPropertyMatchType.EndsWith:
                    query.Where(member => member.Username.EndsWith(usernameToMatch));
                    break;
                case StringPropertyMatchType.Wildcard:
                    query.Where(member => member.Username.SqlWildcard(usernameToMatch, TextColumnType.NVarchar));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchType));
            }
            var matchedMembers = GetByQuery(query).ToArray();

            var membersInGroup = new List<IMember>();
            //then we need to filter the matched members that are in the role
            //since the max sql params are 2100 on sql server, we'll reduce that to be safe for potentially other servers and run the queries in batches
            var inGroups = matchedMembers.InGroupsOf(1000);
            foreach (var batch in inGroups)
            {
                var memberIdBatch = batch.Select(x => x.Id);
                var sql = Sql().SelectAll().From<Member2MemberGroupDto>()
                    .Where<Member2MemberGroupDto>(dto => dto.MemberGroup == memberGroup.Id)
                    .Where("Member IN (@memberIds)", new { memberIds = memberIdBatch });
                var memberIdsInGroup = Database.Fetch<Member2MemberGroupDto>(sql)
                    .Select(x => x.Member).ToArray();

                membersInGroup.AddRange(matchedMembers.Where(x => memberIdsInGroup.Contains(x.Id)));
            }

            return membersInGroup;

        }

        /// <summary>
        /// Get all members in a specific group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetByMemberGroup(string groupName)
        {
            var grpQry = Query<IMemberGroup>().Where(group => group.Name.Equals(groupName));
            var memberGroup = _memberGroupRepository.GetByQuery(grpQry).FirstOrDefault();
            if (memberGroup == null) return Enumerable.Empty<IMember>();

            var subQuery = Sql().Select("Member").From<Member2MemberGroupDto>().Where<Member2MemberGroupDto>(dto => dto.MemberGroup == memberGroup.Id);

            var sql = GetBaseQuery(false)
                //TODO: An inner join would be better, though I've read that the query optimizer will always turn a
                // subquery with an IN clause into an inner join anyways.
                .Append("WHERE umbracoNode.id IN (" + subQuery.SQL + ")", subQuery.Arguments)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<MemberDto>(sql));

        }

        public bool Exists(string username)
        {
            var sql = Sql()
                .SelectCount()
                .From<MemberDto>()
                .Where<MemberDto>(x => x.LoginName == username);

            return Database.ExecuteScalar<int>(sql) > 0;
        }

        public int GetCountByQuery(IQuery<IMember> query)
        {
            var sqlWithProps = GetNodeIdQueryWithPropertyData();
            var translator = new SqlTranslator<IMember>(sqlWithProps, query);
            var sql = translator.Translate();

            //get the COUNT base query
            var fullSql = GetBaseQuery(true)
                .Append(new Sql("WHERE umbracoNode.id IN (" + sql.SQL + ")", sql.Arguments));

            return Database.ExecuteScalar<int>(fullSql);
        }

        /// <summary>
        /// Gets paged member results
        /// </summary>
        /// <param name="query">
        /// The where clause, if this is null all records are queried
        /// </param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="orderBy">The order by column</param>
        /// <param name="orderDirection">The order direction.</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search query</param>
        /// <returns></returns>
        /// <remarks>
        /// The query supplied will ONLY work with data specifically on the cmsMember table because we are using NPoco paging (SQL paging)
        /// </remarks>
        public IEnumerable<IMember> GetPagedResultsByQuery(IQuery<IMember> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMember> filter = null, bool newest = true)
        {
            Sql<ISqlContext> filterSql = null;
            if (filter != null)
            {
                filterSql = Sql();
                foreach (var clause in filter.GetWhereClauses())
                {
                    filterSql = filterSql.Append($"AND ({clause.Item1})", clause.Item2);
                }
            }

            return GetPagedResultsByQuery<MemberDto>(query, pageIndex, pageSize, out totalRecords,
                x => MapDtosToContent(x), orderBy, orderDirection, orderBySystemField, "cmsMember",
                filterSql);
        }

        private string _pagedResultsByQueryWhere;

        private string GetPagedResultsByQueryWhere()
        {
            if (_pagedResultsByQueryWhere == null)
                _pagedResultsByQueryWhere = " AND ("
                    + $"({SqlSyntax.GetQuotedTableName("umbracoNode")}.{SqlSyntax.GetQuotedColumnName("text")} LIKE @0)"
                    + " OR "
                    + $"({SqlSyntax.GetQuotedTableName("cmsMember")}.{SqlSyntax.GetQuotedColumnName("LoginName")} LIKE @0)"
                    + ")";

            return _pagedResultsByQueryWhere;
        }

        protected override string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            //Some custom ones
            switch (orderBy.ToUpperInvariant())
            {
                case "EMAIL":
                    return GetDatabaseFieldNameForOrderBy("cmsMember", "email");
                case "LOGINNAME":
                    return GetDatabaseFieldNameForOrderBy("cmsMember", "loginName");
                case "USERNAME":
                    return GetDatabaseFieldNameForOrderBy("cmsMember", "loginName");
            }

            return base.GetDatabaseFieldNameForOrderBy(orderBy);
        }

        private IEnumerable<IMember> MapDtosToContent(List<MemberDto> dtos, bool withCache = false)
        {
            var temps = new List<TempContent<Member>>();
            var contentTypes = new Dictionary<int, IMemberType>();
            var content = new Member[dtos.Count];

            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                var versionId = dto.ContentVersionDto.VersionId;

                if (withCache)
                {
                    // if the cache contains the (proper version of the) item, use it
                    var cached = IsolatedCache.GetCacheItem<IMember>(GetCacheIdKey<IMember>(dto.NodeId));
                    if (cached != null && cached.Version == dto.ContentVersionDto.VersionId)
                    {
                        content[i] = (Member) cached; // fixme should we just cache Content not IContent?
                        continue;
                    }
                }

                // else, need to build it

                // get the content type - the repository is full cache *but* still deep-clones
                // whatever comes out of it, so use our own local index here to avoid this
                var contentTypeId = dto.ContentDto.ContentTypeId;
                if (contentTypes.TryGetValue(contentTypeId, out var contentType) == false)
                    contentTypes[contentTypeId] = contentType = _memberTypeRepository.Get(contentTypeId);

                var c = content[i] = MemberFactory.BuildEntity(dto, contentType);

                // need properties
                temps.Add(new TempContent<Member>(dto.NodeId, versionId, contentType, c));
            }

            // load all properties for all documents from database in 1 query - indexed by version id
            var properties = GetPropertyCollections(temps);

            // assign properites
            foreach (var temp in temps)
            {
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }

            return content;
        }

        private IMember MapDtoToContent(MemberDto dto)
        {
            var memberType = _memberTypeRepository.Get(dto.ContentDto.ContentTypeId);
            var member = MemberFactory.BuildEntity(dto, memberType);

            // get properties - indexed by version id
            var temp = new TempContent<Member>(dto.ContentDto.NodeId, dto.ContentVersionDto.VersionId, memberType);
            var properties = GetPropertyCollections(new List<TempContent<Member>> { temp });
            member.Properties = properties[dto.ContentVersionDto.VersionId];

            // clear dirty props on init - U4-1943
            member.ResetDirtyProperties(false);
            return member;
        }

        private int GetNewChildSortOrder(int parentId, int first)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetSortOrder", tsql =>
                tsql.Select($"COALESCE(MAX(sortOrder),{first - 1})").From<NodeDto>().Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId") && x.NodeObjectType == NodeObjectTypeId)
            );
            return Database.ExecuteScalar<int>(template.Sql(parentId)) + 1; // fixme can we mix named & non-named?
        }
    }
}
