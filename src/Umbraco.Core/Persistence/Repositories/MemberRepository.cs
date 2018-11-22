using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMember"/>
    /// </summary>
    internal class MemberRepository : VersionableRepositoryBase<int, IMember>, IMemberRepository
    {
        private readonly IMemberTypeRepository _memberTypeRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMemberGroupRepository _memberGroupRepository;
        private readonly ContentXmlRepository<IMember> _contentXmlRepository;
        private readonly ContentPreviewRepository<IMember> _contentPreviewRepository;

        public MemberRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IMemberTypeRepository memberTypeRepository, IMemberGroupRepository memberGroupRepository, ITagRepository tagRepository, IContentSection contentSection)
            : base(work, cache, logger, sqlSyntax, contentSection)
        {
            if (memberTypeRepository == null) throw new ArgumentNullException("memberTypeRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _memberTypeRepository = memberTypeRepository;
            _tagRepository = tagRepository;
            _memberGroupRepository = memberGroupRepository;
            _contentXmlRepository = new ContentXmlRepository<IMember>(work, CacheHelper.NoCache, logger, sqlSyntax);
            _contentPreviewRepository = new ContentPreviewRepository<IMember>(work, CacheHelper.NoCache, logger, sqlSyntax);
        }

        #region Overrides of RepositoryBase<int, IMembershipUser>

        protected override IMember PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

            var dto = Database.Fetch<MemberDto, ContentVersionDto, ContentDto, NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateMemberFromDto(dto, sql);

            return content;

        }

        protected override IEnumerable<IMember> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { ids = ids });
            }

            return ProcessQuery(sql, new PagingSqlQuery(sql));

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

                baseQuery.Append(new Sql("WHERE umbracoNode.id IN (" + sql.SQL + ")", sql.Arguments))
                    .OrderBy<NodeDto>(x => x.SortOrder);

                return ProcessQuery(baseQuery, new PagingSqlQuery(baseQuery));
            }
            else
            {
                var translator = new SqlTranslator<IMember>(baseQuery, query);
                var sql = translator.Translate()
                    .OrderBy<NodeDto>(x => x.SortOrder);

                return ProcessQuery(sql, new PagingSqlQuery(sql));
            }

        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMembershipUser>

        protected override Sql GetBaseQuery(BaseQueryType queryType)
        {
            var sql = new Sql();
            sql.Select(queryType == BaseQueryType.Count ? "COUNT(*)" : (queryType == BaseQueryType.Ids ? "cmsMember.nodeId" : "*"))
                .From<MemberDto>(SqlSyntax)
                .InnerJoin<ContentVersionDto>(SqlSyntax)
                .On<ContentVersionDto, MemberDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentDto>(SqlSyntax)
                .On<ContentVersionDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                //We're joining the type so we can do a query against the member type - not sure if this adds much overhead or not?
                // the execution plan says it doesn't so we'll go with that and in that case, it might be worth joining the content
                // types by default on the document and media repo's so we can query by content type there too.
                .InnerJoin<ContentTypeDto>(SqlSyntax)
                .On<ContentTypeDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId, SqlSyntax);
            return sql;
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? BaseQueryType.Count : BaseQueryType.FullSingle);
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected Sql GetNodeIdQueryWithPropertyData()
        {
            var sql = new Sql();
            sql.Select("DISTINCT(umbracoNode.id)")
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
            return sql;
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

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Member); }
        }

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
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = ((IUmbracoEntity)entity).ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = ((IUmbracoEntity)entity).ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

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

            UpdatePropertyTags(entity, _tagRepository);

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
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = ((IUmbracoEntity)entity).ParentId });
                ((IUmbracoEntity)entity).Path = string.Concat(parent.Path, ",", entity.Id);
                ((IUmbracoEntity)entity).Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = ((IUmbracoEntity)entity).ParentId, NodeObjectType = NodeObjectTypeId });
                ((IUmbracoEntity)entity).SortOrder = maxSortOrder + 1;
            }

            var factory = new MemberFactory(NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            var o = Database.Update(nodeDto);

            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != ((Member)entity).ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentVersionDto.ContentDto;
                Database.Update(newContentDto);
            }

            //In order to update the ContentVersion we need to retrieve its primary key id
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { Version = entity.Version });
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

            UpdatePropertyTags(entity, _tagRepository);

            dirtyEntity.ResetDirtyProperties();
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IMembershipUser>

        public override IEnumerable<IMember> GetAllVersions(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);
            return ProcessQuery(sql, new PagingSqlQuery(sql), true);
        }        

        public void RebuildXmlStructures(Func<IMember, XElement> serializer, int groupSize = 200, IEnumerable<int> contentTypeIds = null)
        {
            // the previous way of doing this was to run it all in one big transaction,
            // and to bulk-insert groups of xml rows - which works, until the transaction
            // times out - and besides, because v7 transactions are ReadCommited, it does
            // not bring much safety - so this reverts to updating each record individually,
            // and it may be slower in the end, but should be more resilient.

            var baseId = 0;
            var contentTypeIdsA = contentTypeIds == null ? new int[0] : contentTypeIds.ToArray();
            while (true)
            {
                // get the next group of nodes
                var query = GetBaseQuery(false);
                if (contentTypeIdsA.Length > 0)
                    query = query
                        .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIdsA, SqlSyntax);
                query = query
                    .Where<NodeDto>(x => x.NodeId > baseId)
                    .OrderBy<NodeDto>(x => x.NodeId, SqlSyntax);
                var sql = SqlSyntax.SelectTop(query, groupSize);
                var xmlItems = ProcessQuery(sql, new PagingSqlQuery(sql))
                    .Select(x => new ContentXmlDto { NodeId = x.Id, Xml = serializer(x).ToString() })
                    .ToList();

                // no more nodes, break
                if (xmlItems.Count == 0) break;

                foreach (var xmlItem in xmlItems)
                {
                    try
                    {
                        // InsertOrUpdate tries to update first, which is good since it is what
                        // should happen in most cases, then it tries to insert, and it should work
                        // unless the node has been deleted, and we just report the exception
                        Database.InsertOrUpdate(xmlItem);
                    }
                    catch (Exception e)
                    {
                        Logger.Error<MediaRepository>("Could not rebuild XML for nodeId=" + xmlItem.NodeId, e);
                    }
                }
                baseId = xmlItems.Last().NodeId;
            }
        }

        public override IMember GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<MemberDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var memberType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new MemberFactory(memberType, NodeObjectTypeId, dto.NodeId);
            var member = factory.BuildEntity(dto);

            var properties = GetPropertyCollection(new PagingSqlQuery(sql), new[] { new DocumentDefinition(dto.ContentVersionDto, memberType) });

            member.Properties = properties[dto.ContentVersionDto.VersionId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)member).ResetDirtyProperties(false);
            return member;

        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE contentNodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            //get the group id
            var grpQry = new Query<IMemberGroup>().Where(group => group.Name.Equals(roleName));
            var memberGroup = _memberGroupRepository.GetByQuery(grpQry).FirstOrDefault();
            if (memberGroup == null) return Enumerable.Empty<IMember>();

            // get the members by username
            var query = new Query<IMember>();
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
                    throw new ArgumentOutOfRangeException("matchType");
            }
            var matchedMembers = GetByQuery(query).ToArray();

            var membersInGroup = new List<IMember>();
            //then we need to filter the matched members that are in the role
            //since the max sql params are 2100 on sql server, we'll reduce that to be safe for potentially other servers and run the queries in batches
            var inGroups = matchedMembers.InGroupsOf(1000);
            foreach (var batch in inGroups)
            {
                var memberIdBatch = batch.Select(x => x.Id);
                var sql = new Sql().Select("*").From<Member2MemberGroupDto>()
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
            var grpQry = new Query<IMemberGroup>().Where(group => group.Name.Equals(groupName));
            var memberGroup = _memberGroupRepository.GetByQuery(grpQry).FirstOrDefault();
            if (memberGroup == null) return Enumerable.Empty<IMember>();

            var subQuery = new Sql().Select("Member").From<Member2MemberGroupDto>().Where<Member2MemberGroupDto>(dto => dto.MemberGroup == memberGroup.Id);

            var sql = GetBaseQuery(false)
                //TODO: An inner join would be better, though I've read that the query optimizer will always turn a
                // subquery with an IN clause into an inner join anyways.
                .Append(new Sql("WHERE umbracoNode.id IN (" + subQuery.SQL + ")", subQuery.Arguments))
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            return ProcessQuery(sql, new PagingSqlQuery(sql));

        }

        public bool Exists(string username)
        {
            var sql = new Sql();

            sql.Select("COUNT(*)")
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
        /// The query supplied will ONLY work with data specifically on the cmsMember table because we are using PetaPoco paging (SQL paging)
        /// </remarks>
        public IEnumerable<IMember> GetPagedResultsByQuery(IQuery<IMember> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMember> filter = null)
        {
            var filterSql = new Sql();
            if (filter != null)
            {
                foreach (var filterClause in filter.GetWhereClauses())
                {
                    filterSql.Append(string.Format("AND ({0})", filterClause.Item1), filterClause.Item2);
                }
            }

            Func<Tuple<string, object[]>> filterCallback = () => new Tuple<string, object[]>(filterSql.SQL, filterSql.Arguments);            

            return GetPagedResultsByQuery<MemberDto>(query, pageIndex, pageSize, out totalRecords,
                new Tuple<string, string>("cmsMember", "nodeId"),
                (sqlFull, sqlIds) => ProcessQuery(sqlFull, sqlIds), orderBy, orderDirection, orderBySystemField,
                filterCallback);
        }

        public void AddOrUpdateContentXml(IMember content, Func<IMember, XElement> xml)
        {
            _contentXmlRepository.AddOrUpdate(new ContentXmlEntity<IMember>(content, xml));
        }

        public void AddOrUpdatePreviewXml(IMember content, Func<IMember, XElement> xml)
        {
            _contentPreviewRepository.AddOrUpdate(new ContentPreviewEntity<IMember>(content, xml));
        }

        protected override string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            //Some custom ones
            switch (orderBy.ToUpperInvariant())
            {
                case "EMAIL":
                    return "cmsMember.Email";
                case "LOGINNAME":
                    return "cmsMember.LoginName";
            }

            return base.GetDatabaseFieldNameForOrderBy(orderBy);
        }

        protected override string GetEntityPropertyNameForOrderBy(string orderBy)
        {
            //Some custom ones
            switch (orderBy.ToUpperInvariant())
            {
                case "LOGINNAME":
                    return "Username";
            }

            return base.GetEntityPropertyNameForOrderBy(orderBy);
        }

        /// <summary>
        /// This is the underlying method that processes most queries for this repository
        /// </summary>
        /// <param name="sqlFull">
        /// The full SQL to select all member data 
        /// </param>
        /// <param name="pagingSqlQuery">
        /// The Id SQL to just return all member ids - used to process the properties for the member item
        /// </param>
        /// <param name="withCache"></param>
        /// <returns></returns>
        private IEnumerable<IMember> ProcessQuery(Sql sqlFull, PagingSqlQuery pagingSqlQuery, bool withCache = false)
        {
            // fetch returns a list so it's ok to iterate it in this method
            var dtos = Database.Fetch<MemberDto, ContentVersionDto, ContentDto, NodeDto>(sqlFull);

            //This is a tuple list identifying if the content item came from the cache or not
            var content = new List<Tuple<IMember, bool>>();
            var defs = new DocumentDefinitionCollection();

            foreach (var dto in dtos)
            {
                // if the cache contains the item, use it
                if (withCache)
                {
                    var cached = IsolatedCache.GetCacheItem<IMember>(GetCacheIdKey<IMember>(dto.NodeId));
                    //only use this cached version if the dto returned is the same version - this is just a safety check, members dont 
                    //store different versions, but just in case someone corrupts some data we'll double check to be sure.
                    if (cached != null && cached.Version == dto.ContentVersionDto.VersionId)
                    {
                        content.Add(new Tuple<IMember, bool>(cached, true));
                        continue;
                    }
                }

                // else, need to fetch from the database
                // content type repository is full-cache so OK to get each one independently
                var contentType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

                // need properties
                if (defs.AddOrUpdate(new DocumentDefinition(dto.ContentVersionDto, contentType)))
                {
                    content.Add(new Tuple<IMember, bool>(MemberFactory.BuildEntity(dto, contentType), false));
                }
            }

            // load all properties for all documents from database in 1 query
            var propertyData = GetPropertyCollection(pagingSqlQuery, defs);

            // assign property data
            foreach (var contentItem in content)
            {
                var cc = contentItem.Item1;
                var fromCache = contentItem.Item2;

                //if this has come from cache, we do not need to build up it's structure
                if (fromCache) continue;

                cc.Properties = propertyData[cc.Version];

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                cc.ResetDirtyProperties(false);
            }

            return content.Select(x => x.Item1).ToArray();
        }

        /// <summary>
        /// Private method to create a member object from a MemberDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="docSql"></param>
        /// <returns></returns>
        private IMember CreateMemberFromDto(MemberDto dto, Sql docSql)
        {
            var memberType = _memberTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new MemberFactory(memberType, NodeObjectTypeId, dto.ContentVersionDto.NodeId);
            var member = factory.BuildEntity(dto);

            var docDef = new DocumentDefinition(dto.ContentVersionDto, memberType);

            var properties = GetPropertyCollection(docSql, new[] { docDef });

            member.Properties = properties[dto.ContentVersionDto.VersionId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)member).ResetDirtyProperties(false);
            return member;
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _memberTypeRepository.Dispose();
            _tagRepository.Dispose();
            _memberGroupRepository.Dispose();
            _contentXmlRepository.Dispose();
            _contentPreviewRepository.Dispose();
        }

    }
}
