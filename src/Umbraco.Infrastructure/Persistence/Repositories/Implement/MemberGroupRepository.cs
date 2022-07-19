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

internal class MemberGroupRepository : EntityRepositoryBase<int, IMemberGroup>, IMemberGroupRepository
{
    private readonly IEventMessagesFactory _eventMessagesFactory;

    public MemberGroupRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MemberGroupRepository> logger,
        IEventMessagesFactory eventMessagesFactory)
        : base(scopeAccessor, cache, logger) =>
        _eventMessagesFactory = eventMessagesFactory;

    protected Guid NodeObjectTypeId => Constants.ObjectTypes.MemberGroup;

    public IMemberGroup? Get(Guid uniqueId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where<NodeDto>(x => x.UniqueId == uniqueId);

        NodeDto? dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

        return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
    }

    public IMemberGroup? GetByName(string? name) =>
        IsolatedCache.GetCacheItem(
            typeof(IMemberGroup).FullName + "." + name,
            () =>
            {
                IQuery<IMemberGroup> qry = Query<IMemberGroup>().Where(group => group.Name!.Equals(name));
                IEnumerable<IMemberGroup> result = Get(qry);
                return result.FirstOrDefault();
            },

            // cache for 5 mins since that is the default in the Runtime app cache
            TimeSpan.FromMinutes(5),

            // sliding is true
            true);

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

    public IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId)
    {
        Sql<ISqlContext> sql = Sql()
            .Select("umbracoNode.*")
            .From<NodeDto>()
            .InnerJoin<Member2MemberGroupDto>()
            .On<NodeDto, Member2MemberGroupDto>(dto => dto.NodeId, dto => dto.MemberGroup)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
            .Where<Member2MemberGroupDto>(x => x.Member == memberId);

        return Database.Fetch<NodeDto>(sql)
            .DistinctBy(dto => dto.NodeId)
            .Select(x => MemberGroupFactory.BuildEntity(x));
    }

    public IEnumerable<IMemberGroup> GetMemberGroupsForMember(string? username)
    {
        Sql<ISqlContext>? sql = Sql()
            .Select("un.*")
            .From("umbracoNode AS un")
            .InnerJoin("cmsMember2MemberGroup")
            .On("cmsMember2MemberGroup.MemberGroup = un.id")
            .InnerJoin("cmsMember")
            .On("cmsMember.nodeId = cmsMember2MemberGroup.Member")
            .Where("un.nodeObjectType=@objectType", new { objectType = NodeObjectTypeId })
            .Where("cmsMember.LoginName=@loginName", new { loginName = username });

        return Database.Fetch<NodeDto>(sql)
            .DistinctBy(dto => dto.NodeId)
            .Select(x => MemberGroupFactory.BuildEntity(x));
    }

    public void ReplaceRoles(int[] memberIds, string[] roleNames) => AssignRolesInternal(memberIds, roleNames, true);

    public void AssignRoles(int[] memberIds, string[] roleNames) => AssignRolesInternal(memberIds, roleNames);

    public void DissociateRoles(int[] memberIds, string[] roleNames) => DissociateRolesInternal(memberIds, roleNames);

    protected override IMemberGroup? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });

        NodeDto? dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

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

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Node}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new[]
        {
            "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",
            "DELETE FROM umbracoUserGroup2Node WHERE nodeId = @id",
            "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @id",
            "DELETE FROM umbracoRelation WHERE parentId = @id", "DELETE FROM umbracoRelation WHERE childId = @id",
            "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
            "DELETE FROM cmsMember2MemberGroup WHERE MemberGroup = @id", "DELETE FROM umbracoNode WHERE id = @id",
        };
        return list;
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
            .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " in (@names)", new { names = roleNames });
        IEnumerable<string?> existingRoles = Database.Fetch<NodeDto>(existingSql).Select(x => x.Text);
        IEnumerable<string?> missingRoles = roleNames.Except(existingRoles, StringComparer.CurrentCultureIgnoreCase);
        MemberGroup[] missingGroups = missingRoles.Select(x => new MemberGroup { Name = x }).ToArray();

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
            Database.Execute("DELETE FROM cmsMember2MemberGroup WHERE Member IN (@memberIds)", new { memberIds });

            currentlyAssigned = Array.Empty<AssignedRolesDto>();
        }
        else
        {
            // get the groups that are currently assigned to any of these members
            Sql<ISqlContext> assignedSql = Sql()
                .Select(
                    $"{SqlSyntax.GetQuotedColumnName("text")},{SqlSyntax.GetQuotedColumnName("Member")},{SqlSyntax.GetQuotedColumnName("MemberGroup")}")
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
        Sql<ISqlContext>? existingSql = Sql()
            .SelectAll()
            .From<NodeDto>()
            .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
            .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " in (@names)", new { names = roleNames });
        var existingRolesIds = Database.Fetch<NodeDto>(existingSql).Select(x => x.NodeId).ToArray();

        Database.Execute(
            "DELETE FROM cmsMember2MemberGroup WHERE Member IN (@memberIds) AND MemberGroup IN (@memberGroups)",
            new
            {
                /*memberIds =*/
                memberIds,
                memberGroups = existingRolesIds,
            });
    }

    private class AssignedRolesDto
    {
        [Column("text")]
        public string? RoleName { get; set; }

        [Column("Member")]
        public int MemberId { get; set; }

        [Column("MemberGroup")]
        public int MemberGroupId { get; set; }
    }
}
