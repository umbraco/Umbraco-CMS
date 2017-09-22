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

        #region Overrides of RepositoryBase<int, IMembershipUser>

        protected override IMember PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<MemberDto>(sql.SelectTop(1)).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateMemberFromDto(dto, dto.ContentVersionDto.VersionId);

            return content;

        }

        protected override IEnumerable<IMember> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { /*ids =*/ ids });
            }

            return MapQueryDtos(Database.Fetch<MemberDto>(sql));

        }

        protected override IEnumerable<IMember> PerformGetByQuery(IQuery<IMember> query)
        {
            var baseQuery = GetBaseQuery(false);

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

                return MapQueryDtos(Database.Fetch<MemberDto>(baseQuery));
            }
            else
            {
                var translator = new SqlTranslator<IMember>(baseQuery, query);
                var sql = translator.Translate()
                    .OrderBy<NodeDto>(x => x.SortOrder);

                return MapQueryDtos(Database.Fetch<MemberDto>(sql));
            }

        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,IMembershipUser>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);
        }

        protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType)
        {
            var sql = Sql();

            switch (queryType)
            {
                case QueryType.Count:
                    sql = sql.SelectCount();
                    break;
                case QueryType.Ids:
                    sql = sql.Select("cmsMember.nodeId");
                    break;
                case QueryType.Many:
                case QueryType.Single:
                    sql = sql.Select<MemberDto>(r =>
                        r.Select(x => x.ContentVersionDto, r1 =>
                            r1.Select(x => x.ContentDto, r2 =>
                                r2.Select(x => x.NodeDto))));
                    break;
            }

            sql
                .From<MemberDto>()
                .InnerJoin<ContentVersionDto>()
                .On<ContentVersionDto, MemberDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                //We're joining the type so we can do a query against the member type - not sure if this adds much overhead or not?
                // the execution plan says it doesn't so we'll go with that and in that case, it might be worth joining the content
                // types by default on the document and media repo's so we can query by content type there too.
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

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
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
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
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsMember2MemberGroup WHERE Member = @Id",
                               "DELETE FROM cmsMember WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeId = @Id",
                               "DELETE FROM cmsContent WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Member;

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMember entity)
        {
            ((Member)entity).AddingEntity();

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            var factory = new MemberFactory(NodeObjectTypeId, entity.Id);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { /*ParentId =*/ entity.ParentId });
            var level = parent.Level + 1;
            var sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { /*ParentId =*/ entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var unused = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            //Create the Content specific data - cmsContent
            var contentDto = dto.ContentVersionDto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            //Create the first version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            dto.ContentVersionDto.NodeId = nodeDto.NodeId;
            Database.Insert(dto.ContentVersionDto);

            //Create the first entry in cmsMember
            dto.NodeId = nodeDto.NodeId;

            //if the password is empty, generate one with the special prefix
            //this will hash the guid with a salt so should be nicely random
            if (entity.RawPasswordValue.IsNullOrWhiteSpace())
            {
                var aspHasher = new PasswordHasher();
                dto.Password = Constants.Security.EmptyPasswordPrefix +
                               aspHasher.HashPassword(Guid.NewGuid().ToString("N"));
                //re-assign
                entity.RawPasswordValue = dto.Password;
            }

            Database.Insert(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType.CompositionPropertyTypes.ToArray(), entity.Version, entity.Id);
            //Add Properties
            // - don't try to save the property if it doesn't exist (or doesn't have an ID) on the content type
            // - this can occur if the member type doesn't contain the built-in properties that the
            // - member object contains.
            var propsToPersist = entity.Properties.Where(x => x.PropertyType.HasIdentity).ToArray();
            var propertyDataDtos = propertyFactory.BuildDto(propsToPersist);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                var primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));
                keyDictionary.Add(propertyDataDto.PropertyTypeId, primaryKey);
            }

            //Update Properties with its newly set Id
            foreach (var property in propsToPersist)
            {
                property.Id = keyDictionary[property.PropertyTypeId];
            }

            UpdateEntityTags(entity, _tagRepository);

            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            ((Member)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMember entity)
        {
            //Updates Modified date
            ((Member)entity).UpdatingEntity();

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            var dirtyEntity = (ICanBeDirty)entity;

            //Look up parent to get and set the correct Path and update SortOrder if ParentId has changed
            if (dirtyEntity.IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { /*ParentId =*/ entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { /*ParentId =*/ entity.ParentId, NodeObjectType = NodeObjectTypeId });
                entity.SortOrder = maxSortOrder + 1;
            }

            var factory = new MemberFactory(NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { /*Id =*/ entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            var unused = Database.Update(nodeDto);

            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != ((Member)entity).ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentVersionDto.ContentDto;
                Database.Update(newContentDto);
            }

            //In order to update the ContentVersion we need to retrieve its primary key id
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { /*Version =*/ entity.Version });
            dto.ContentVersionDto.Id = contentVerDto.Id;
            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
            Database.Update(dto.ContentVersionDto);

            //Updates the cmsMember entry if it has changed

            //NOTE: these cols are the REAL column names in the db
            var changedCols = new List<string>();

            if (dirtyEntity.IsPropertyDirty("Email"))
            {
                changedCols.Add("Email");
            }
            if (dirtyEntity.IsPropertyDirty("Username"))
            {
                changedCols.Add("LoginName");
            }
            // DO NOT update the password if it has not changed or if it is null or empty
            if (dirtyEntity.IsPropertyDirty("RawPasswordValue") && entity.RawPasswordValue.IsNullOrWhiteSpace() == false)
            {
                changedCols.Add("Password");
            }
            //only update the changed cols
            if (changedCols.Count > 0)
            {
                Database.Update(dto, changedCols);
            }

            //TODO ContentType for the Member entity

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType.CompositionPropertyTypes.ToArray(), entity.Version, entity.Id);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            // - don't try to save the property if it doesn't exist (or doesn't have an ID) on the content type
            // - this can occur if the member type doesn't contain the built-in properties that the
            // - member object contains.
            var propsToPersist = entity.Properties.Where(x => x.PropertyType.HasIdentity).ToArray();

            var propertyDataDtos = propertyFactory.BuildDto(propsToPersist);

            foreach (var propertyDataDto in propertyDataDtos)
            {
                if (propertyDataDto.Id > 0)
                {
                    Database.Update(propertyDataDto);
                }
                else
                {
                    int primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));
                    keyDictionary.Add(propertyDataDto.PropertyTypeId, primaryKey);
                }
            }

            //Update Properties with its newly set Id
            if (keyDictionary.Any())
            {
                foreach (var property in ((Member)entity).Properties)
                {
                    if (keyDictionary.ContainsKey(property.PropertyTypeId) == false) continue;

                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            UpdateEntityTags(entity, _tagRepository);

            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            dirtyEntity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IMember entity)
        {
            // raise event first else potential FK issues
            OnUowRemovingEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));
            base.PersistDeletedItem(entity);
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IMembershipUser>

        public override IEnumerable<IMember> GetAllVersions(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            return MapQueryDtos(Database.Fetch<MemberDto>(sql), true);
        }

        public override IMember GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<MemberDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var memberType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);
            var member = MemberFactory.BuildEntity(dto, memberType);

            var properties = GetPropertyCollection(new List<TempContent> { new TempContent(dto.NodeId, dto.ContentVersionDto.VersionId, member.UpdateDate, member.CreateDate, memberType) });

            member.Properties = properties[dto.ContentVersionDto.VersionId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)member).ResetDirtyProperties(false);
            return member;

        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            // raise event first else potential FK issues
            OnUowRemovingVersion(new UnitOfWorkVersionEventArgs(UnitOfWork, id, versionId));

            Database.Delete<PropertyDataDto>("WHERE contentNodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
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

            return MapQueryDtos(Database.Fetch<MemberDto>(sql));

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
                x => MapQueryDtos(x), orderBy, orderDirection, orderBySystemField, "cmsMember",
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

        private IEnumerable<IMember> MapQueryDtos(List<MemberDto> dtos, bool withCache = false)
        {
            var content = new IMember[dtos.Count];
            var temps = new List<TempContent>();
            var contentTypes = new Dictionary<int, IMemberType>();

            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];

                if (withCache)
                {
                    // if the cache contains the (proper version of the) item, use it
                    var cached = IsolatedCache.GetCacheItem<IMember>(GetCacheIdKey<IMember>(dto.NodeId));
                    if (cached != null && cached.Version == dto.ContentVersionDto.VersionId)
                    {
                        content[i] = cached;
                        continue;
                    }
                }

                // else, need to fetch from the database

                // get the content type - the repository is full cache *but* still deep-clones
                // whatever comes out of it, so use our own local index here to avoid this
                if (contentTypes.TryGetValue(dto.ContentVersionDto.ContentDto.ContentTypeId, out IMemberType contentType) == false)
                    contentTypes[dto.ContentVersionDto.ContentDto.ContentTypeId] = contentType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

                // fixme
                //
                // 7.6 ProcessQuery has an additional 'allVersions' flag that is false by default, meaning
                // we should always get the latest version of each content item. meaning what what we
                // are processing now is a more recent version than what we already processed, we need to
                // replace
                // but it has flaws: it's not dealing with what could be in the cache (should be the latest
                // version, always) and it's not replacing the content that is already in the list...
                // so considering it broken, not implementing now, MUST FIX

                var c = content[i] = MemberFactory.BuildEntity(dto, contentType);

                // need properties
                temps.Add(new TempContent(
                    dto.NodeId,
                    dto.ContentVersionDto.VersionId,
                    dto.ContentVersionDto.VersionDate,
                    dto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                    contentType,
                    c
                ));
            }

            // load all properties for all documents from database in 1 query
            var propertyData = GetPropertyCollection(temps);

            // assign
            foreach (var temp in temps)
            {
                temp.Content.Properties = propertyData[temp.Version];

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((Entity) temp.Content).ResetDirtyProperties(false);
            }

            return content;
        }

        /// <summary>
        /// Private method to create a member object from a MemberDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        private IMember CreateMemberFromDto(MemberDto dto, Guid versionId)
        {
            var memberType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);
            var member = MemberFactory.BuildEntity(dto, memberType);
            var temp = new TempContent(dto.ContentVersionDto.NodeId, versionId, member.UpdateDate, member.CreateDate, memberType);

            var properties = GetPropertyCollection(new List<TempContent> { temp });
            member.Properties = properties[dto.ContentVersionDto.VersionId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)member).ResetDirtyProperties(false);
            return member;
        }
    }
}
