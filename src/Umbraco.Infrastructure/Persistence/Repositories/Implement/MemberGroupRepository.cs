using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
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

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class MemberGroupRepository : EntityRepositoryBase<int, IMemberGroup>, IMemberGroupRepository
    {
        private readonly IEventMessagesFactory _eventMessagesFactory;

        public MemberGroupRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MemberGroupRepository> logger, IEventMessagesFactory eventMessagesFactory)
            : base(scopeAccessor, cache, logger) =>
            _eventMessagesFactory = eventMessagesFactory;

        protected override IMemberGroup? PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id = id });

            var dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
        }

        protected override IEnumerable<IMemberGroup> PerformGetAll(params int[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);

            return Database.Fetch<NodeDto>(sql).Select(x => MemberGroupFactory.BuildEntity(x));
        }
        protected override async Task<IEnumerable<IMemberGroup>> PerformGetAllAsync(params int[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);

            return (await Database.FetchAsync<NodeDto>(sql)).Select(x => MemberGroupFactory.BuildEntity(x));
        }

        private Sql<ISqlContext> GetAllSql(int[]? ids)
        {
            var sql = Sql()
                .SelectAll()
                .From<NodeDto>()
                .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);

            if (ids?.Any() ?? false)
                sql.WhereIn<NodeDto>(x => x.NodeId, ids);
            return sql;
        }

        protected override IEnumerable<IMemberGroup> PerformGetByQuery(IQuery<IMemberGroup> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMemberGroup>(sqlClause, query);
            var sql = translator.Translate();

            return Database.Fetch<NodeDto>(sql).Select(x => MemberGroupFactory.BuildEntity(x));
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<NodeDto>();

            sql
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Cms.Core.Constants.DatabaseSchema.Tables.Node}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new[]
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",
                               "DELETE FROM umbracoUserGroup2Node WHERE nodeId = @id",
                               "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @id",
                               "DELETE FROM umbracoRelation WHERE parentId = @id",
                               "DELETE FROM umbracoRelation WHERE childId = @id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
                               "DELETE FROM cmsMember2MemberGroup WHERE MemberGroup = @id",
                               "DELETE FROM umbracoNode WHERE id = @id"
                           };
            return list;
        }

        protected Guid NodeObjectTypeId => Cms.Core.Constants.ObjectTypes.MemberGroup;

        protected override void PersistNewItem(IMemberGroup entity)
        {
            //Save to db
            entity.AddingEntity();
            var group = (MemberGroup)entity;
            var dto = MemberGroupFactory.BuildDto(group);
            var o = Database.IsNew(dto) ? Convert.ToInt32(Database.Insert(dto)) : Database.Update(dto);
            group.Id = dto.NodeId; //Set Id on entity to ensure an Id is set

            //Update with new correct path and id
            dto.Path = string.Concat("-1,", dto.NodeId);
            Database.Update(dto);
            //assign to entity
            group.Id = o;
            group.ResetDirtyProperties();
        }

        protected override async Task PersistNewItemAsync(IMemberGroup entity)
        {
            //Save to db
            entity.AddingEntity();
            var group = (MemberGroup)entity;
            var dto = MemberGroupFactory.BuildDto(group);
            var o = Database.IsNew(dto) ? Convert.ToInt32(await Database.InsertAsync(dto)) : await Database.UpdateAsync(dto);
            group.Id = dto.NodeId; //Set Id on entity to ensure an Id is set

            //Update with new correct path and id
            dto.Path = string.Concat("-1,", dto.NodeId);
            await Database.UpdateAsync(dto);
            //assign to entity
            group.Id = o;
            group.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMemberGroup entity)
        {
            var dto = MemberGroupFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        protected override async Task PersistUpdatedItemAsync(IMemberGroup entity)
        {
            var dto = MemberGroupFactory.BuildDto(entity);

            await Database.UpdateAsync(dto);

            entity.ResetDirtyProperties();
        }

        public IMemberGroup? Get(Guid uniqueId)
        {
            var sql = GetBaseQuery(false);
            sql.Where<NodeDto>(x => x.UniqueId == uniqueId);

            var dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
        }

        public IMemberGroup? GetByName(string? name)
        {
            return IsolatedCache.GetCacheItem<IMemberGroup>(
                typeof(IMemberGroup).FullName + "." + name,
                () =>
                {
                    var qry = Query<IMemberGroup>().Where(group => group.Name!.Equals(name));
                    var result = Get(qry);
                    return result?.FirstOrDefault();
                },
                //cache for 5 mins since that is the default in the Runtime app cache
                TimeSpan.FromMinutes(5),
                //sliding is true
                true);
        }

        public IMemberGroup? CreateIfNotExists(string roleName)
        {
            var qry = Query<IMemberGroup>().Where(group => group.Name!.Equals(roleName));
            var result = Get(qry);

            if (result?.Any() ?? false)
                return null;

            var grp = new MemberGroup
            {
                Name = roleName
            };
            PersistNewItem(grp);

            var evtMsgs = _eventMessagesFactory.Get();
            if (AmbientScope.Notifications.PublishCancelable(new MemberGroupSavingNotification(grp, evtMsgs)))
            {
                return null;
            }

            AmbientScope.Notifications.Publish(new MemberGroupSavedNotification(grp, evtMsgs));

            return grp;
        }

        public IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId)
        {
            var sql = Sql()
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
            var sql = Sql()
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

        private void AssignRolesInternal(int[] memberIds, string[] roleNames, bool replace = false)
        {
            //ensure they're unique
            memberIds = memberIds.Distinct().ToArray();

            //create the missing roles first

            Sql<ISqlContext> existingSql = Sql()
               .SelectAll()
               .From<NodeDto>()
               .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
               .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " in (@names)", new { names = roleNames });
            IEnumerable<string?> existingRoles = Database.Fetch<NodeDto>(existingSql).Select(x => x.Text);
            IEnumerable<string?> missingRoles = roleNames.Except(existingRoles, StringComparer.CurrentCultureIgnoreCase);
            MemberGroup[] missingGroups = missingRoles.Select(x => new MemberGroup { Name = x }).ToArray();

            var evtMsgs = _eventMessagesFactory.Get();
            if (AmbientScope.Notifications.PublishCancelable(new MemberGroupSavingNotification(missingGroups, evtMsgs)))
            {
                return;
            }

            foreach (MemberGroup m in missingGroups)
            {
                PersistNewItem(m);
            }

            AmbientScope.Notifications.Publish(new MemberGroupSavedNotification(missingGroups, evtMsgs));

            //now go get all the dto's for roles with these role names
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
                //get the groups that are currently assigned to any of these members

                Sql<ISqlContext> assignedSql = Sql()
                    .Select($"{SqlSyntax.GetQuotedColumnName("text")},{SqlSyntax.GetQuotedColumnName("Member")},{SqlSyntax.GetQuotedColumnName("MemberGroup")}")
                    .From<NodeDto>()
                    .InnerJoin<Member2MemberGroupDto>()
                    .On<NodeDto, Member2MemberGroupDto>(dto => dto.NodeId, dto => dto.MemberGroup)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                    .WhereIn<Member2MemberGroupDto>(x => x.Member, memberIds);

                currentlyAssigned = Database.Fetch<AssignedRolesDto>(assignedSql).ToArray();
            }

            //assign the roles for each member id

            foreach (var memberId in memberIds)
            {
                //find any roles for the current member that are currently assigned that
                //exist in the roleNames list, then determine which ones are not currently assigned.
                var mId = memberId;
                AssignedRolesDto[] found = currentlyAssigned.Where(x => x.MemberId == mId).ToArray();
                IEnumerable<string?> assignedRoles = found.Where(x => roleNames.Contains(x.RoleName, StringComparer.CurrentCultureIgnoreCase)).Select(x => x.RoleName);
                IEnumerable<string?> nonAssignedRoles = roleNames.Except(assignedRoles, StringComparer.CurrentCultureIgnoreCase);

                IEnumerable<Member2MemberGroupDto> dtos = nonAssignedRoles
                    .Select(x => new Member2MemberGroupDto
                    {
                        Member = mId,
                        MemberGroup = rolesForNames[x!].NodeId
                    });

                Database.InsertBulk(dtos);
            }
        }

        public void DissociateRoles(int[] memberIds, string[] roleNames)
        {
            DissociateRolesInternal(memberIds, roleNames);
        }

        private void DissociateRolesInternal(int[] memberIds, string[] roleNames)
        {
            var existingSql = Sql()
                    .SelectAll()
                    .From<NodeDto>()
                    .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
                    .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " in (@names)", new { names = roleNames });
            var existingRolesIds = Database.Fetch<NodeDto>(existingSql).Select(x => x.NodeId).ToArray();

            Database.Execute("DELETE FROM cmsMember2MemberGroup WHERE Member IN (@memberIds) AND MemberGroup IN (@memberGroups)",
                new { /*memberIds =*/ memberIds, memberGroups = existingRolesIds });
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
}
