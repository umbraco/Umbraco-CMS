using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IMember" />
/// </summary>
public class MemberRepository : ContentRepositoryBase<int, IMember, MemberRepository>, IMemberRepository
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly MemberRepositoryUsernameCachePolicy _memberByUsernameCachePolicy;
    private readonly IMemberGroupRepository _memberGroupRepository;
    private readonly IMemberTypeRepository _memberTypeRepository;
    private readonly MemberPasswordConfigurationSettings _passwordConfiguration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITagRepository _tagRepository;
    private bool _passwordConfigInitialized;
    private string? _passwordConfigJson;

    public MemberRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MemberRepository> logger,
        IMemberTypeRepository memberTypeRepository,
        IMemberGroupRepository memberGroupRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        IPasswordHasher passwordHasher,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        IOptions<MemberPasswordConfigurationSettings> passwordConfiguration)
        : base(scopeAccessor, cache, logger, languageRepository, relationRepository, relationTypeRepository,
            propertyEditors, dataValueReferenceFactories, dataTypeService, eventAggregator)
    {
        _memberTypeRepository =
            memberTypeRepository ?? throw new ArgumentNullException(nameof(memberTypeRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _passwordHasher = passwordHasher;
        _jsonSerializer = serializer;
        _memberGroupRepository = memberGroupRepository;
        _passwordConfiguration = passwordConfiguration.Value;
        _memberByUsernameCachePolicy =
            new MemberRepositoryUsernameCachePolicy(GlobalIsolatedCache, ScopeAccessor, DefaultOptions);
    }

    /// <summary>
    ///     Returns a serialized dictionary of the password configuration that is stored against the member in the database
    /// </summary>
    private string? DefaultPasswordConfigJson
    {
        get
        {
            if (_passwordConfigInitialized)
            {
                return _passwordConfigJson;
            }

            var passwordConfig = new PersistedPasswordSettings
            {
                HashAlgorithm = _passwordConfiguration.HashAlgorithmType
            };

            _passwordConfigJson = passwordConfig == null ? null : _jsonSerializer.Serialize(passwordConfig);
            _passwordConfigInitialized = true;
            return _passwordConfigJson;
        }
    }

    protected override MemberRepository This => this;

    public override int RecycleBinId => throw new NotSupportedException();

    public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch,
        StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
    {
        //get the group id
        IQuery<IMemberGroup> grpQry = Query<IMemberGroup>().Where(group => group.Name!.Equals(roleName));
        IMemberGroup? memberGroup = _memberGroupRepository.Get(grpQry)?.FirstOrDefault();
        if (memberGroup == null)
        {
            return Enumerable.Empty<IMember>();
        }

        // get the members by username
        IQuery<IMember> query = Query<IMember>();
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

        IMember[] matchedMembers = Get(query).ToArray();

        var membersInGroup = new List<IMember>();

        // Then we need to filter the matched members that are in the role
        foreach (IEnumerable<int> group in matchedMembers.Select(x => x.Id)
                     .InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<Member2MemberGroupDto>()
                .Where<Member2MemberGroupDto>(dto => dto.MemberGroup == memberGroup.Id)
                .WhereIn<Member2MemberGroupDto>(dto => dto.Member, group);

            var memberIdsInGroup = Database.Fetch<Member2MemberGroupDto>(sql)
                .Select(x => x.Member).ToArray();

            membersInGroup.AddRange(matchedMembers.Where(x => memberIdsInGroup.Contains(x.Id)));
        }

        return membersInGroup;
    }

    /// <summary>
    ///     Get all members in a specific group
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public IEnumerable<IMember> GetByMemberGroup(string groupName)
    {
        IQuery<IMemberGroup> grpQry = Query<IMemberGroup>().Where(group => group.Name!.Equals(groupName));
        IMemberGroup? memberGroup = _memberGroupRepository.Get(grpQry)?.FirstOrDefault();
        if (memberGroup == null)
        {
            return Enumerable.Empty<IMember>();
        }

        Sql<ISqlContext> subQuery = Sql().Select("Member").From<Member2MemberGroupDto>()
            .Where<Member2MemberGroupDto>(dto => dto.MemberGroup == memberGroup.Id);

        Sql<ISqlContext> sql = GetBaseQuery(false)
            // TODO: An inner join would be better, though I've read that the query optimizer will always turn a
            // subquery with an IN clause into an inner join anyways.
            .Append("WHERE umbracoNode.id IN (" + subQuery.SQL + ")", subQuery.Arguments)
            .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
            .OrderBy<NodeDto>(x => x.SortOrder);

        return MapDtosToContent(Database.Fetch<MemberDto>(sql));
    }

    public bool Exists(string username)
    {
        Sql<ISqlContext> sql = Sql()
            .SelectCount()
            .From<MemberDto>()
            .Where<MemberDto>(x => x.LoginName == username);

        return Database.ExecuteScalar<int>(sql) > 0;
    }

    public int GetCountByQuery(IQuery<IMember>? query)
    {
        Sql<ISqlContext> sqlWithProps = GetNodeIdQueryWithPropertyData();
        var translator = new SqlTranslator<IMember>(sqlWithProps, query);
        Sql<ISqlContext> sql = translator.Translate();

        //get the COUNT base query
        Sql<ISqlContext> fullSql = GetBaseQuery(true)
            .Append(new Sql("WHERE umbracoNode.id IN (" + sql.SQL + ")", sql.Arguments));

        return Database.ExecuteScalar<int>(fullSql);
    }

    public async Task<PagedModel<IMember>> GetPagedByFilterAsync(MemberFilter memberFilter, int skip, int take, Ordering? ordering = null)
    {
        Sql<ISqlContext> sql = Sql().Select<NodeDto>(x => x.NodeId)
            .From<NodeDto>()
            .InnerJoin<MemberDto>().On<NodeDto, MemberDto>((n, m) => n.NodeId == m.NodeId);

        if (memberFilter.MemberTypeId.HasValue)
        {
            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((memberNode, memberContent) => memberContent.NodeId == memberNode.NodeId)
                .InnerJoin<NodeDto>("mtn").On<NodeDto, ContentDto>((memberTypeNode, memberContent) => memberContent.ContentTypeId == memberTypeNode.NodeId && memberTypeNode.UniqueId == memberFilter.MemberTypeId, "mtn");
        }

        if (memberFilter.MemberGroupName.IsNullOrWhiteSpace() is false)
        {
            sql = sql
                .InnerJoin<Member2MemberGroupDto>().On<MemberDto, Member2MemberGroupDto>((m, memberToGroup) => m.NodeId == memberToGroup.Member)
                .InnerJoin<NodeDto>("mgn").On<NodeDto, Member2MemberGroupDto>((memberGroupNode, memberToGroup) => memberToGroup.MemberGroup == memberGroupNode.NodeId && memberGroupNode.Text == memberFilter.MemberGroupName, "mgn");
        }

        if (memberFilter.IsApproved is not null)
        {
            sql = sql.Where<MemberDto>(member => member.IsApproved == memberFilter.IsApproved);
        }

        if (memberFilter.IsLockedOut is not null)
        {
            sql = sql.Where<MemberDto>(member => member.IsLockedOut == memberFilter.IsLockedOut);
        }

        if (memberFilter.Filter is not null)
        {
            var whereClauses = new List<Func<Sql<ISqlContext>, Sql<ISqlContext>>>()
            {
                (x) => x.Where<NodeDto>(memberNode => memberNode.Text != null && memberNode.Text.Contains(memberFilter.Filter)),
                (x) => x.Where<MemberDto>(memberNode => memberNode.Email.Contains(memberFilter.Filter)),
                (x) => x.Where<MemberDto>(memberNode => memberNode.LoginName.Contains(memberFilter.Filter)),
            };

            if (int.TryParse(memberFilter.Filter, out int filterAsIntId))
            {
                whereClauses.Add((x) => x.Where<NodeDto>(memberNode => memberNode.NodeId == filterAsIntId));
            }

            if (Guid.TryParse(memberFilter.Filter, out Guid filterAsGuid))
            {
                whereClauses.Add((x) => x.Where<NodeDto>(memberNode => memberNode.UniqueId == filterAsGuid));
            }

            sql = sql.WhereAny(whereClauses.ToArray());
        }

        if (ordering is not null)
        {
            ApplyOrdering(ref sql, ordering);
        }

        var pageIndex = skip / take;
        Page<MemberDto>? pageResult = await Database.PageAsync<MemberDto>(pageIndex+1, take, sql);

        // shortcut so our join is not too big, but we also hope these are cached, so we don't have to map them again.
        var nodeIds = pageResult.Items.Select(x => x.NodeId).ToArray();

        return new PagedModel<IMember>(pageResult.TotalItems, nodeIds.Any() ? GetMany(nodeIds) : Array.Empty<IMember>());
    }

    private void ApplyOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(ordering);

        if (ordering.OrderBy.IsNullOrWhiteSpace())
        {
            return;
        }

        var orderBy = ordering.OrderBy.ToLowerInvariant() switch
        {
            "username" => sql.GetAliasedField(SqlSyntax.GetFieldName<MemberDto>(x => x.LoginName)),
            "name" => sql.GetAliasedField(SqlSyntax.GetFieldName<NodeDto>(x => x.Text)),
            "email" => sql.GetAliasedField(SqlSyntax.GetFieldName<MemberDto>(x => x.Email)),
            _ => throw new NotSupportedException("Ordering not supported"),
        };

        if (ordering.Direction == Direction.Ascending)
        {
            sql.OrderBy(orderBy);
        }
        else
        {
            sql.OrderByDescending(orderBy);
        }
    }

    /// <summary>
    ///     Gets paged member results.
    /// </summary>
    public override IEnumerable<IMember> GetPage(
        IQuery<IMember>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IMember>? filter,
        Ordering? ordering)
    {
        Sql<ISqlContext>? filterSql = null;

        if (filter != null)
        {
            filterSql = Sql();
            foreach (Tuple<string, object[]> clause in filter.GetWhereClauses())
            {
                filterSql = filterSql.Append($"AND ({clause.Item1})", clause.Item2);
            }
        }

        return GetPage<MemberDto>(query, pageIndex, pageSize, out totalRecords,
            x => MapDtosToContent(x),
            filterSql,
            ordering);
    }

    public IMember? GetByUsername(string? username) =>
        _memberByUsernameCachePolicy.GetByUserName(CacheKeys.MemberUserNameCachePrefix, username, PerformGetByUsername, PerformGetAllByUsername);

    public int[] GetMemberIds(string[] usernames)
    {
        Guid memberObjectType = Constants.ObjectTypes.Member;

        Sql<ISqlContext> memberSql = Sql()
            .Select("umbracoNode.id")
            .From<NodeDto>()
            .InnerJoin<MemberDto>()
            .On<NodeDto, MemberDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == memberObjectType)
            .Where("cmsMember.LoginName in (@usernames)", new
            {
                /*usernames =*/
                usernames
            });
        return Database.Fetch<int>(memberSql).ToArray();
    }

    protected override string ApplySystemOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
    {
        if (ordering.OrderBy.InvariantEquals("email"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.Email);
        }

        if (ordering.OrderBy.InvariantEquals("loginName"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.LoginName);
        }

        if (ordering.OrderBy.InvariantEquals("userName"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.LoginName);
        }

        if (ordering.OrderBy.InvariantEquals("updateDate"))
        {
            return SqlSyntax.GetFieldName<ContentVersionDto>(x => x.VersionDate);
        }

        if (ordering.OrderBy.InvariantEquals("createDate"))
        {
            return SqlSyntax.GetFieldName<NodeDto>(x => x.CreateDate);
        }

        if (ordering.OrderBy.InvariantEquals("contentTypeAlias"))
        {
            return SqlSyntax.GetFieldName<ContentTypeDto>(x => x.Alias);
        }

        if (ordering.OrderBy.InvariantEquals("failedPasswordAttempts"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.FailedPasswordAttempts);
        }

        if (ordering.OrderBy.InvariantEquals("approved"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.IsApproved);
        }

        if (ordering.OrderBy.InvariantEquals("lockedOut"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.IsLockedOut);
        }

        if (ordering.OrderBy.InvariantEquals("lastLockoutDate"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.LastLockoutDate);
        }

        if (ordering.OrderBy.InvariantEquals("lastLoginDate"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.LastLoginDate);
        }

        if (ordering.OrderBy.InvariantEquals("lastPasswordChangeDate"))
        {
            return SqlSyntax.GetFieldName<MemberDto>(x => x.LastPasswordChangeDate);
        }

        return base.ApplySystemOrdering(ref sql, ordering);
    }

    private IEnumerable<IMember> MapDtosToContent(List<MemberDto> dtos, bool withCache = false)
    {
        var temps = new List<TempContent<Member>>();
        var contentTypes = new Dictionary<int, IMemberType?>();
        var content = new Member[dtos.Count];

        for (var i = 0; i < dtos.Count; i++)
        {
            MemberDto dto = dtos[i];

            if (withCache)
            {
                // if the cache contains the (proper version of the) item, use it
                IMember? cached =
                    IsolatedCache.GetCacheItem<IMember>(RepositoryCacheKeys.GetKey<IMember, int>(dto.NodeId));
                if (cached != null && cached.VersionId == dto.ContentVersionDto.Id)
                {
                    content[i] = (Member)cached;
                    continue;
                }
            }

            // else, need to build it

            // get the content type - the repository is full cache *but* still deep-clones
            // whatever comes out of it, so use our own local index here to avoid this
            var contentTypeId = dto.ContentDto.ContentTypeId;
            if (contentTypes.TryGetValue(contentTypeId, out IMemberType? contentType) == false)
            {
                contentTypes[contentTypeId] = contentType = _memberTypeRepository.Get(contentTypeId);
            }

            Member c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

            // need properties
            var versionId = dto.ContentVersionDto.Id;
            temps.Add(new TempContent<Member>(dto.NodeId, versionId, 0, contentType, c));
        }

        // load all properties for all documents from database in 1 query - indexed by version id
        IDictionary<int, PropertyCollection> properties = GetPropertyCollections(temps);

        // assign properties
        foreach (TempContent<Member> temp in temps)
        {
            if (temp.Content is not null)
            {
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }
        }

        return content;
    }

    private IMember MapDtoToContent(MemberDto dto)
    {
        IMemberType? memberType = _memberTypeRepository.Get(dto.ContentDto.ContentTypeId);
        Member member = ContentBaseFactory.BuildEntity(dto, memberType);

        // get properties - indexed by version id
        var versionId = dto.ContentVersionDto.Id;
        var temp = new TempContent<Member>(dto.ContentDto.NodeId, versionId, 0, memberType);
        IDictionary<int, PropertyCollection> properties =
            GetPropertyCollections(new List<TempContent<Member>> {temp});
        member.Properties = properties[versionId];

        // reset dirty initial properties (U4-1946)
        member.ResetDirtyProperties(false);
        return member;
    }

    private IMember? PerformGetByUsername(string? username)
    {
        IQuery<IMember> query = Query<IMember>().Where(x => x.Username.Equals(username));
        return PerformGetByQuery(query).FirstOrDefault();
    }

    private IEnumerable<IMember> PerformGetAllByUsername(params string[]? usernames)
    {
        IQuery<IMember> query = Query<IMember>().WhereIn(x => x.Username, usernames);
        return PerformGetByQuery(query);
    }

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Member;

    protected override IMember? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<NodeDto>(x => x.NodeId == id)
            .SelectTop(1);

        MemberDto? dto = Database.Fetch<MemberDto>(sql).FirstOrDefault();
        return dto == null
            ? null
            : MapDtoToContent(dto);
    }

    protected override IEnumerable<IMember> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, ids);
        }

        return MapDtosToContent(Database.Fetch<MemberDto>(sql));
    }

    protected override IEnumerable<IMember> PerformGetByQuery(IQuery<IMember> query)
    {
        Sql<ISqlContext> baseQuery = GetBaseQuery(false);

        // TODO: why is this different from content/media?!
        // check if the query is based on properties or not

        IEnumerable<Tuple<string, object[]>> wheres = query.GetWhereClauses();
        //this is a pretty rudimentary check but will work, we just need to know if this query requires property
        // level queries
        if (wheres.Any(x => x.Item1.Contains("cmsPropertyType")))
        {
            Sql<ISqlContext> sqlWithProps = GetNodeIdQueryWithPropertyData();
            var translator = new SqlTranslator<IMember>(sqlWithProps, query);
            Sql<ISqlContext> sql = translator.Translate();

            baseQuery.Append("WHERE umbracoNode.id IN (" + sql.SQL + ")", sql.Arguments)
                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<MemberDto>(baseQuery));
        }
        else
        {
            var translator = new SqlTranslator<IMember>(baseQuery, query);
            Sql<ISqlContext> sql = translator.Translate()
                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<MemberDto>(sql));
        }
    }

    protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType) => GetBaseQuery(queryType, true);

    protected virtual Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        switch (queryType) // TODO: pretend we still need these queries for now
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
                                r1.Select(x => x.NodeDto)))

                    // ContentRepositoryBase expects a variantName field to order by name
                    // so get it here, though for members it's just the plain node name
                    .AndSelect<NodeDto>(x => Alias(x.Text, "variantName"));
                break;
        }

        sql
            .From<MemberDto>()
            .InnerJoin<ContentDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)

            // joining the type so we can do a query against the member type - not sure if this adds much overhead or not?
            // the execution plan says it doesn't so we'll go with that and in that case, it might be worth joining the content
            // types by default on the document and media repos so we can query by content type there too.
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>(left => left.ContentTypeId, right => right.NodeId);

        sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        if (current)
        {
            sql.Where<ContentVersionDto>(x => x.Current); // always get the current version
        }

        return sql;
    }

    protected override void PersistDeletedItem(IMember entity)
    {
        _memberByUsernameCachePolicy.DeleteByUserName(CacheKeys.MemberUserNameCachePrefix, entity.Username);
        base.PersistDeletedItem(entity);
    }

    // TODO: move that one up to Versionable! or better: kill it!
    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);

    protected override string GetBaseWhereClause() // TODO: can we kill / refactor this?
        => "umbracoNode.id = @id";

    // TODO: document/understand that one
    protected Sql<ISqlContext> GetNodeIdQueryWithPropertyData() =>
        Sql()
            .Select("DISTINCT(umbracoNode.id)")
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<MemberDto>().On<ContentDto, MemberDto>((left, right) => left.NodeId == right.NodeId)
            .LeftJoin<PropertyTypeDto>()
            .On<ContentDto, PropertyTypeDto>(left => left.ContentTypeId, right => right.ContentTypeId)
            .LeftJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId)
            .LeftJoin<PropertyDataDto>().On(x => x
                .Where<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id)
                .Where<PropertyDataDto, ContentVersionDto>((left, right) => left.VersionId == right.Id))
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",

            "DELETE FROM umbracoUserGroup2Permission WHERE userGroupKey IN (SELECT [umbracoUserGroup].[Key] FROM umbracoUserGroup WHERE Id = @id)",
            "DELETE FROM umbracoUserGroup2GranularPermission WHERE userGroupKey IN (SELECT [umbracoUserGroup].[Key] FROM umbracoUserGroup WHERE Id = @id)",
            "DELETE FROM umbracoRelation WHERE parentId = @id",
            "DELETE FROM umbracoRelation WHERE childId = @id",
            "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData +
            " WHERE versionId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion +
            " WHERE nodeId = @id)",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.ExternalLoginToken} WHERE externalLoginId = (SELECT id FROM {Constants.DatabaseSchema.Tables.ExternalLogin} WHERE userOrMemberKey = (SELECT uniqueId from {Constants.DatabaseSchema.Tables.Node} where id = @id))",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.ExternalLogin} WHERE userOrMemberKey = (SELECT uniqueId from {Constants.DatabaseSchema.Tables.Node} where id = @id)",
            "DELETE FROM cmsMember2MemberGroup WHERE Member = @id",
            "DELETE FROM cmsMember WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @id",
            "DELETE FROM umbracoNode WHERE id = @id"
        };
        return list;
    }

    #endregion

    #region Versions

    public override IEnumerable<IMember> GetAllVersions(int nodeId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many, false)
            .Where<NodeDto>(x => x.NodeId == nodeId)
            .OrderByDescending<ContentVersionDto>(x => x.Current)
            .AndByDescending<ContentVersionDto>(x => x.VersionDate);

        return MapDtosToContent(Database.Fetch<MemberDto>(sql), true);
    }

    public override IMember? GetVersion(int versionId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<ContentVersionDto>(x => x.Id == versionId);

        MemberDto? dto = Database.Fetch<MemberDto>(sql).FirstOrDefault();
        return dto == null ? null : MapDtoToContent(dto);
    }

    protected override void PerformDeleteVersion(int id, int versionId)
    {
        Database.Delete<PropertyDataDto>("WHERE versionId = @VersionId", new {versionId});
        Database.Delete<ContentVersionDto>("WHERE versionId = @VersionId", new {versionId});
    }

    #endregion

    #region Persist

    protected override void PersistNewItem(IMember entity)
    {
        entity.AddingEntity();

        // ensure security stamp if missing
        if (entity.SecurityStamp.IsNullOrWhiteSpace())
        {
            entity.SecurityStamp = Guid.NewGuid().ToString();
        }

        // ensure that strings don't contain characters that are invalid in xml
        // TODO: do we really want to keep doing this here?
        entity.SanitizeEntityPropertiesForXmlStorage();

        // create the dto
        MemberDto memberDto = ContentBaseFactory.BuildDto(entity);

        // check if we have a user config else use the default
        memberDto.PasswordConfig = entity.PasswordConfiguration ?? DefaultPasswordConfigJson;

        // derive path and level from parent
        NodeDto parent = GetParentNodeDto(entity.ParentId);
        var level = parent.Level + 1;

        // get sort order
        var sortOrder = GetNewChildSortOrder(entity.ParentId, 0);

        // persist the node dto
        NodeDto nodeDto = memberDto.ContentDto.NodeDto;
        nodeDto.Path = parent.Path;
        nodeDto.Level = Convert.ToInt16(level);
        nodeDto.SortOrder = sortOrder;

        // see if there's a reserved identifier for this unique id
        // and then either update or insert the node dto
        var id = GetReservedId(nodeDto.UniqueId);
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
        ContentDto contentDto = memberDto.ContentDto;
        contentDto.NodeId = nodeDto.NodeId;
        Database.Insert(contentDto);

        // persist the content version dto
        // assumes a new version id and version date (modified date) has been set
        ContentVersionDto contentVersionDto = memberDto.ContentVersionDto;
        contentVersionDto.NodeId = nodeDto.NodeId;
        contentVersionDto.Current = true;
        Database.Insert(contentVersionDto);
        entity.VersionId = contentVersionDto.Id;

        // persist the member dto
        memberDto.NodeId = nodeDto.NodeId;

        // if the password is empty, generate one with the special prefix
        // this will hash the guid with a salt so should be nicely random
        if (entity.RawPasswordValue.IsNullOrWhiteSpace())
        {
            memberDto.Password = Constants.Security.EmptyPasswordPrefix +
                                 _passwordHasher.HashPassword(Guid.NewGuid().ToString("N"));
            entity.RawPasswordValue = memberDto.Password;
        }

        Database.Insert(memberDto);

        // persist the property data
        InsertPropertyValues(entity, 0, out _, out _);

        SetEntityTags(entity, _tagRepository, _jsonSerializer);

        OnUowRefreshedEntity(new MemberRefreshNotification(entity, new EventMessages()));

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IMember entity)
    {
        // update
        entity.UpdatingEntity();

        // ensure security stamp if missing
        if (entity.SecurityStamp.IsNullOrWhiteSpace())
        {
            entity.SecurityStamp = Guid.NewGuid().ToString();
        }

        // ensure that strings don't contain characters that are invalid in xml
        // TODO: do we really want to keep doing this here?
        entity.SanitizeEntityPropertiesForXmlStorage();

        // if parent has changed, get path, level and sort order
        if (entity.IsPropertyDirty("ParentId"))
        {
            NodeDto parent = GetParentNodeDto(entity.ParentId);

            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;
            entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
        }

        // create the dto
        MemberDto memberDto = ContentBaseFactory.BuildDto(entity);

        // update the node dto
        NodeDto nodeDto = memberDto.ContentDto.NodeDto;
        Database.Update(nodeDto);

        // update the content dto
        Database.Update(memberDto.ContentDto);

        // update the content version dto
        Database.Update(memberDto.ContentVersionDto);

        // update the member dto
        // but only the changed columns, 'cos we cannot update password if empty
        var changedCols = new List<string>();

        if (entity.IsPropertyDirty("SecurityStamp"))
        {
            changedCols.Add("securityStampToken");
        }

        if (entity.IsPropertyDirty("Email"))
        {
            changedCols.Add("Email");
        }

        if (entity.IsPropertyDirty("Username"))
        {
            changedCols.Add("LoginName");
        }

        if (entity.IsPropertyDirty(nameof(entity.FailedPasswordAttempts)))
        {
            changedCols.Add(nameof(entity.FailedPasswordAttempts));
        }

        if (entity.IsPropertyDirty(nameof(entity.IsApproved)))
        {
            changedCols.Add(nameof(entity.IsApproved));
        }

        if (entity.IsPropertyDirty(nameof(entity.IsLockedOut)))
        {
            changedCols.Add(nameof(entity.IsLockedOut));
        }

        if (entity.IsPropertyDirty(nameof(entity.LastLockoutDate)))
        {
            changedCols.Add(nameof(entity.LastLockoutDate));
        }

        if (entity.IsPropertyDirty(nameof(entity.LastLoginDate)))
        {
            changedCols.Add(nameof(entity.LastLoginDate));
        }

        if (entity.IsPropertyDirty(nameof(entity.LastPasswordChangeDate)))
        {
            changedCols.Add(nameof(entity.LastPasswordChangeDate));
        }

        // this can occur from an upgrade
        if (memberDto.PasswordConfig.IsNullOrWhiteSpace())
        {
            memberDto.PasswordConfig = DefaultPasswordConfigJson;
            changedCols.Add("passwordConfig");
        }
        else if (memberDto.PasswordConfig == Constants.Security.UnknownPasswordConfigJson)
        {
            changedCols.Add("passwordConfig");
        }

        // do NOT update the password if it has not changed or if it is null or empty
        if (entity.IsPropertyDirty("RawPasswordValue") && !string.IsNullOrWhiteSpace(entity.RawPasswordValue))
        {
            changedCols.Add("Password");

            // If the security stamp hasn't already updated we need to force it
            if (entity.IsPropertyDirty("SecurityStamp") == false)
            {
                memberDto.SecurityStampToken = entity.SecurityStamp = Guid.NewGuid().ToString();
                changedCols.Add("securityStampToken");
            }

            // check if we have a user config else use the default
            memberDto.PasswordConfig = entity.PasswordConfiguration ?? DefaultPasswordConfigJson;
            changedCols.Add("passwordConfig");
        }

        if (entity.IsPropertyDirty("EmailConfirmedDate"))
        {
            changedCols.Add("emailConfirmedDate");
        }

        // If userlogin or the email has changed then need to reset security stamp
        if (changedCols.Contains("Email") || changedCols.Contains("LoginName"))
        {
            memberDto.EmailConfirmedDate = null;
            changedCols.Add("emailConfirmedDate");

            // If the security stamp hasn't already updated we need to force it
            if (entity.IsPropertyDirty("SecurityStamp") == false)
            {
                memberDto.SecurityStampToken = entity.SecurityStamp = Guid.NewGuid().ToString();
                changedCols.Add("securityStampToken");
            }
        }

        if (changedCols.Count > 0)
        {
            Database.Update(memberDto, changedCols);
        }

        ReplacePropertyValues(entity, entity.VersionId, 0, out _, out _);

        SetEntityTags(entity, _tagRepository, _jsonSerializer);

        OnUowRefreshedEntity(new MemberRefreshNotification(entity, new EventMessages()));

        _memberByUsernameCachePolicy.DeleteByUserName(CacheKeys.MemberUserNameCachePrefix, entity.Username);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc/>
    public async Task UpdateLoginPropertiesAsync(IMember member)
    {
        var updatedLastLoginDate = member.IsPropertyDirty(nameof(member.LastLoginDate));
        var updatedSecurityStamp = member.IsPropertyDirty(nameof(member.SecurityStamp));
        if (updatedLastLoginDate is false && updatedSecurityStamp is false)
        {
            return;
        }

        NPocoSqlExtensions.SqlUpd<MemberDto> GetMemberSetExpression(IMember member, NPocoSqlExtensions.SqlUpd<MemberDto> m)
        {
            var setExpression = new NPocoSqlExtensions.SqlUpd<MemberDto>(SqlContext);
            if (updatedLastLoginDate)
            {
                setExpression.Set(x => x.LastLoginDate, member.LastLoginDate);
            }

            if (updatedSecurityStamp)
            {
                setExpression.Set(x => x.SecurityStampToken, member.SecurityStamp);
            }

            return setExpression;
        }

        member.UpdatingEntity();

        Sql<ISqlContext> updateMemberQuery = Sql()
            .Update<MemberDto>(m => GetMemberSetExpression(member, m))
            .Where<MemberDto>(m => m.NodeId == member.Id);
        await Database.ExecuteAsync(updateMemberQuery);

        Sql<ISqlContext> updateContentVersionQuery = Sql()
            .Update<ContentVersionDto>(m => m.Set(x => x.VersionDate, member.UpdateDate))
            .Where<ContentVersionDto>(m => m.NodeId == member.Id && m.Current == true);
        await Database.ExecuteAsync(updateContentVersionQuery);

        OnUowRefreshedEntity(new MemberRefreshNotification(member, new EventMessages()));

        _memberByUsernameCachePolicy.DeleteByUserName(CacheKeys.MemberUserNameCachePrefix, member.Username);

        member.ResetDirtyProperties();
    }

    #endregion
}
