using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class MemberGroupRepository : EntityRepositoryBase<int, IMemberGroup>, IMemberGroupRepository
{
    private readonly IEventMessagesFactory _eventMessagesFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.MemberGroupRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="cache">The application-level caches used for storing and retrieving cached data.</param>
    /// <param name="logger">The logger used for logging repository operations and errors.</param>
    /// <param name="eventMessagesFactory">Factory for creating event messages to be used during repository operations.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public MemberGroupRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MemberGroupRepository> logger,
        IEventMessagesFactory eventMessagesFactory,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService) =>
        _eventMessagesFactory = eventMessagesFactory;

    private Guid NodeObjectTypeId => Constants.ObjectTypes.MemberGroup;

    /// <summary>
    /// Gets the member group with the specified unique identifier.
    /// </summary>
    /// <param name="uniqueId">The unique identifier of the member group.</param>
    /// <returns>The member group if found; otherwise, null.</returns>
    public IMemberGroup? Get(Guid uniqueId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where<NodeDto>(x => x.UniqueId == uniqueId);

        NodeDto? dto = Database.FirstOrDefault<NodeDto>(sql);

        return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
    }

    /// <summary>
    /// Gets a member group by its name.
    /// </summary>
    /// <param name="name">The name of the member group to retrieve.</param>
    /// <returns>The member group matching the specified name, or null if not found.</returns>
    public IMemberGroup? GetByName(string? name) =>
        IsolatedCache.GetCacheItem(
            typeof(IMemberGroup).FullName + "." + name,
            () =>
            {
                IQuery<IMemberGroup> qry = Query<IMemberGroup>().Where(group => group.Name!.Equals(name));
                IEnumerable<IMemberGroup> result = Get(qry);
                return result.FirstOrDefault();
            },

            // use sliding cache with default repository duration
            RepositoryCacheConstants.DefaultCacheDuration,

            // sliding is true
            true);

    /// <summary>
    /// Attempts to create a new member group with the specified role name if one does not already exist.
    /// If a group with the given role name exists, or if the creation is cancelled by a notification, no new group is created.
    /// </summary>
    /// <param name="roleName">The name of the role to use for the new member group.</param>
    /// <returns>
    /// The newly created <see cref="Umbraco.Cms.Core.Models.IMemberGroup"/> if successful; otherwise, <c>null</c> if a group with the specified role name already exists or creation was cancelled.
    /// </returns>
    public IMemberGroup? CreateIfNotExists(string roleName)
    {
        IQuery<IMemberGroup> qry = Query<IMemberGroup>().Where(group => group.Name!.Equals(roleName));
        IEnumerable<IMemberGroup> result = Get(qry);

        if (result.Any())
        {
            return null;
        }

        var grp = new MemberGroup { Name = roleName };
        PersistNewItem(grp);

        EventMessages evtMsgs = _eventMessagesFactory.Get();
        if (AmbientScope.Notifications.PublishCancelable(new MemberGroupSavingNotification(grp, evtMsgs)))
        {
            return null;
        }

        AmbientScope.Notifications.Publish(new MemberGroupSavedNotification(grp, evtMsgs));

        return grp;
    }

    /// <summary>
    /// Gets the member groups associated with the specified member.
    /// </summary>
    /// <param name="memberId">The identifier of the member.</param>
    /// <returns>An enumerable collection of member groups that the member belongs to.</returns>
    public IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId)
    {
        Sql<ISqlContext> sql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .InnerJoin<Member2MemberGroupDto>()
            .On<NodeDto, Member2MemberGroupDto>(dto => dto.NodeId, dto => dto.MemberGroup)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
            .Where<Member2MemberGroupDto>(x => x.Member == memberId);

        return Database.Fetch<NodeDto>(sql)
            .DistinctBy(dto => dto.NodeId)
            .Select(x => MemberGroupFactory.BuildEntity(x));
    }

    /// <summary>
    /// Gets the member groups associated with the specified member.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <returns>An enumerable collection of member groups that the member belongs to.</returns>
    public IEnumerable<IMemberGroup> GetMemberGroupsForMember(string? username)
    {
        Sql<ISqlContext>? sql = Sql()
            .Select($"{QuoteTableName(NodeDto.TableName)}.*")
            .From<NodeDto>()
            .InnerJoin<Member2MemberGroupDto>()
            .On<Member2MemberGroupDto, NodeDto>((g, n) => g.MemberGroup == n.NodeId)
            .InnerJoin<MemberDto>()
            .On<MemberDto, Member2MemberGroupDto>((m, g) => m.NodeId == g.Member)
            .Where<NodeDto>(n => n.NodeObjectType == NodeObjectTypeId)
            .Where<MemberDto>(m => m.LoginName == username);

        return Database.Fetch<NodeDto>(sql)
            .DistinctBy(dto => dto.NodeId)
            .Select(x => MemberGroupFactory.BuildEntity(x));
    }

    /// <summary>
    /// Replaces all existing roles for the specified members with the provided role names.
    /// </summary>
    /// <param name="memberIds">The IDs of the members whose roles will be replaced.</param>
    /// <param name="roleNames">The new set of role names to assign to each member, replacing any existing roles.</param>
    public void ReplaceRoles(int[] memberIds, string[] roleNames) => AssignRolesInternal(memberIds, roleNames, true);

    /// <summary>
    /// Assigns the specified roles to the members identified by the given member IDs.
    /// </summary>
    /// <param name="memberIds">An array of member IDs to assign roles to.</param>
    /// <param name="roleNames">An array of role names to assign to the members.</param>
    public void AssignRoles(int[] memberIds, string[] roleNames) => AssignRolesInternal(memberIds, roleNames);

    /// <summary>
    /// Dissociates the specified roles from the given member IDs.
    /// </summary>
    /// <param name="memberIds">The IDs of the members to dissociate roles from.</param>
    /// <param name="roleNames">The names of the roles to dissociate.</param>
    public void DissociateRoles(int[] memberIds, string[] roleNames) => DissociateRolesInternal(memberIds, roleNames);

    protected override IMemberGroup? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });

        NodeDto? dto = Database.FirstOrDefault<NodeDto>(sql);

        return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
    }

    protected override IEnumerable<IMemberGroup> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, ids);
        }

        return Database.Fetch<NodeDto>(sql).Select(x => MemberGroupFactory.BuildEntity(x));
    }

    protected override IEnumerable<IMemberGroup> PerformGetByQuery(IQuery<IMemberGroup> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IMemberGroup>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        return Database.Fetch<NodeDto>(sql).Select(x => MemberGroupFactory.BuildEntity(x));
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<NodeDto>();

        sql
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuoteTableName(Constants.DatabaseSchema.Tables.Node)}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        Sql<ISqlContext> sql = Sql();

        var inClause = $" IN (SELECT {QuoteTableName("umbracoUserGroup")}.{QuoteColumnName("key")} FROM {QuoteTableName("umbracoUserGroup")} WHERE id = @id)";
        return new List<string>
        {
            $"DELETE FROM {QuoteTableName("umbracoUser2NodeNotify")} WHERE {QuoteColumnName("nodeId")} = @id",
            $"DELETE FROM {QuoteTableName("umbracoUserGroup2Permission")} WHERE {QuoteColumnName("userGroupKey")}{inClause}",
            $"DELETE FROM {QuoteTableName("umbracoUserGroup2GranularPermission")} WHERE {QuoteColumnName("userGroupKey")}{inClause}",
            $"DELETE FROM {QuoteTableName("umbracoRelation")} WHERE {QuoteColumnName("parentId")} = @id",
            $"DELETE FROM {QuoteTableName("umbracoRelation")} WHERE {QuoteColumnName("childId")} = @id",
            $"DELETE FROM {QuoteTableName("cmsTagRelationship")} WHERE {QuoteColumnName("nodeId")} = @id",
            $"DELETE FROM {QuoteTableName("cmsMember2MemberGroup")} WHERE {QuoteColumnName("MemberGroup")} = @id",
            $"DELETE FROM {QuoteTableName("umbracoNode")} WHERE id = @id",
        };
    }

    protected override void PersistNewItem(IMemberGroup entity)
    {
        // Save to db
        entity.AddingEntity();
        var group = (MemberGroup)entity;
        NodeDto dto = MemberGroupFactory.BuildDto(group);
        var o = Database.IsNew(dto) ? Convert.ToInt32(Database.Insert(dto)) : Database.Update(dto);
        group.Id = dto.NodeId; // Set Id on entity to ensure an Id is set

        // Update with new correct path and id
        dto.Path = string.Concat("-1,", dto.NodeId);
        Database.Update(dto);

        // assign to entity
        group.Id = o;
        group.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IMemberGroup entity)
    {
        NodeDto dto = MemberGroupFactory.BuildDto(entity);

        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    private void AssignRolesInternal(int[] memberIds, string[] roleNames, bool replace = false)
    {
        // ensure they're unique
        memberIds = memberIds.Distinct().ToArray();

        // create the missing roles first
        Sql<ISqlContext> existingSql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
            .WhereIn<NodeDto>(n => n.Text, roleNames);
        IEnumerable<string?> existingRoles = Database.Fetch<NodeDto>(existingSql).Select(x => x.Text);
        IEnumerable<string?> missingRoles = roleNames.Except(existingRoles, StringComparer.CurrentCultureIgnoreCase);
        MemberGroup[] missingGroups = [.. missingRoles.Select(x => new MemberGroup { Name = x })];

        EventMessages evtMsgs = _eventMessagesFactory.Get();
        if (AmbientScope.Notifications.PublishCancelable(new MemberGroupSavingNotification(missingGroups, evtMsgs)))
        {
            return;
        }

        foreach (MemberGroup m in missingGroups)
        {
            PersistNewItem(m);
        }

        AmbientScope.Notifications.Publish(new MemberGroupSavedNotification(missingGroups, evtMsgs));

        // now go get all the dto's for roles with these role names
        var rolesForNames = Database.Fetch<NodeDto>(existingSql)
            .ToDictionary(x => x.Text!, StringComparer.InvariantCultureIgnoreCase);

        AssignedRolesDto[] currentlyAssigned;
        if (replace)
        {
            // delete all assigned groups first
            Sql<ISqlContext> delSql = Sql()
                .Delete<Member2MemberGroupDto>()
                .WhereIn<Member2MemberGroupDto>(x => x.Member, memberIds);
            Database.Execute(delSql);

            currentlyAssigned = [];
        }
        else
        {
            // get the groups that are currently assigned to any of these members
            Sql<ISqlContext> assignedSql = Sql()
                .Select($"{QuoteColumnName("text")},{QuoteColumnName("Member")},{QuoteColumnName("MemberGroup")}")
                .From<NodeDto>()
                .InnerJoin<Member2MemberGroupDto>()
                .On<NodeDto, Member2MemberGroupDto>(dto => dto.NodeId, dto => dto.MemberGroup)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .WhereIn<Member2MemberGroupDto>(x => x.Member, memberIds);

            currentlyAssigned = Database.Fetch<AssignedRolesDto>(assignedSql).ToArray();
        }

        // assign the roles for each member id
        foreach (var memberId in memberIds)
        {
            // find any roles for the current member that are currently assigned that
            // exist in the roleNames list, then determine which ones are not currently assigned.
            var mId = memberId;
            AssignedRolesDto[] found = currentlyAssigned.Where(x => x.MemberId == mId).ToArray();
            IEnumerable<string?> assignedRoles = found
                .Where(x => roleNames.Contains(x.RoleName, StringComparer.CurrentCultureIgnoreCase))
                .Select(x => x.RoleName);
            IEnumerable<string?> nonAssignedRoles =
                roleNames.Except(assignedRoles, StringComparer.CurrentCultureIgnoreCase);

            IEnumerable<Member2MemberGroupDto> dtos = nonAssignedRoles
                .Select(x => new Member2MemberGroupDto { Member = mId, MemberGroup = rolesForNames[x!].NodeId });

            Database.InsertBulk(dtos);
        }
    }

    private void DissociateRolesInternal(int[] memberIds, string[] roleNames)
    {
        Sql<ISqlContext> existingSql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
            .WhereIn<NodeDto>(w => w.Text, roleNames);
        var existingRolesIds = Database.Fetch<NodeDto>(existingSql).Select(x => x.NodeId).ToArray();

        Sql<ISqlContext> delSql = Sql()
            .Delete<Member2MemberGroupDto>()
            .WhereIn<Member2MemberGroupDto>(x => x.Member, memberIds)
            .WhereIn<Member2MemberGroupDto>(x => x.MemberGroup, existingRolesIds);
        Database.Execute(delSql);
    }

    private sealed class AssignedRolesDto
    {
        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        [Column("text")]
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the member.
        /// </summary>
        [Column("Member")]
        public int MemberId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the member group.
        /// </summary>
        [Column("MemberGroup")]
        public int MemberGroupId { get; set; }
    }
}
