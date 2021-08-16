using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class MemberGroupRepository : NPocoRepositoryBase<int, IMemberGroup>, IMemberGroupRepository
    {
        public MemberGroupRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IMemberGroup PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id = id });

            var dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
        }

        protected override IEnumerable<IMemberGroup> PerformGetAll(params int[] ids)
        {
            var sql = Sql()
                .SelectAll()
                .From<NodeDto>()
                .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);

            if (ids.Any())
                sql.WhereIn<NodeDto>(x => x.NodeId, ids);

            return Database.Fetch<NodeDto>(sql).Select(x => MemberGroupFactory.BuildEntity(x));
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
            return $"{Constants.DatabaseSchema.Tables.Node}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new[]
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @id",
                               "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @id",
                               "DELETE FROM umbracoRelation WHERE parentId = @id",
                               "DELETE FROM umbracoRelation WHERE childId = @id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @id",
                               "DELETE FROM cmsMember2MemberGroup WHERE MemberGroup = @id",
                               "DELETE FROM umbracoNode WHERE id = @id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MemberGroup;

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

        protected override void PersistUpdatedItem(IMemberGroup entity)
        {
            var dto = MemberGroupFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        public IMemberGroup Get(Guid uniqueId)
        {
            var sql = GetBaseQuery(false);
            sql.Where<NodeDto>(x => x.UniqueId == uniqueId);

            var dto = Database.Fetch<NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            return dto == null ? null : MemberGroupFactory.BuildEntity(dto);
        }

        public IMemberGroup GetByName(string name)
        {
            return IsolatedCache.GetCacheItem<IMemberGroup>(
                typeof (IMemberGroup).FullName + "." + name,
                () =>
                {
                    var qry = Query<IMemberGroup>().Where(group => group.Name.Equals(name));
                    var result = Get(qry);
                    return result.FirstOrDefault();
                },
                //cache for 5 mins since that is the default in the Runtime app cache
                TimeSpan.FromMinutes(5),
                //sliding is true
                true);
        }

        public IMemberGroup CreateIfNotExists(string roleName)
        {
            var qry = Query<IMemberGroup>().Where(group => group.Name.Equals(roleName));
            var result = Get(qry);

            if (result.Any()) return null;

            var grp = new MemberGroup
            {
                Name = roleName
            };
            PersistNewItem(grp);

            if (AmbientScope.Events.DispatchCancelable(SavingMemberGroup, this, new SaveEventArgs<IMemberGroup>(grp)))
                return null;

            AmbientScope.Events.Dispatch(SavedMemberGroup, this, new SaveEventArgs<IMemberGroup>(grp));
            return grp;
        }

        public IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId)
        {
            var sql = Sql()
                .Select("umbracoNode.*")
                .From<NodeDto>()
                .InnerJoin<Member2MemberGroupDto>()
                .On<NodeDto, Member2MemberGroupDto>( dto => dto.NodeId, dto => dto.MemberGroup)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<Member2MemberGroupDto>(x => x.Member == memberId);

            return Database.Fetch<NodeDto>(sql)
                .DistinctBy(dto => dto.NodeId)
                .Select(x => MemberGroupFactory.BuildEntity(x));
        }

        public IEnumerable<IMemberGroup> GetMemberGroupsForMember(string username)
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

        public int[] GetMemberIds(string[] usernames)
        {
            var memberObjectType = Constants.ObjectTypes.Member;

            var memberSql = Sql()
                .Select("umbracoNode.id")
                .From<NodeDto>()
                .InnerJoin<MemberDto>()
                .On<NodeDto, MemberDto>(dto => dto.NodeId, dto => dto.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == memberObjectType)
                .Where("cmsMember.LoginName in (@usernames)", new { /*usernames =*/ usernames });
            return Database.Fetch<int>(memberSql).ToArray();
        }

        public void AssignRoles(string[] usernames, string[] roleNames)
        {
            AssignRolesInternal(GetMemberIds(usernames), roleNames);
        }

        public void DissociateRoles(string[] usernames, string[] roleNames)
        {
            DissociateRolesInternal(GetMemberIds(usernames), roleNames);
        }

        public void AssignRoles(int[] memberIds, string[] roleNames)
        {
            AssignRolesInternal(memberIds, roleNames);
        }

        public void AssignRolesInternal(int[] memberIds, string[] roleNames)
        {
            //ensure they're unique
            memberIds = memberIds.Distinct().ToArray();

            //create the missing roles first

            var existingSql = Sql()
               .SelectAll()
               .From<NodeDto>()
               .Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId)
               .Where("umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " in (@names)", new { names = roleNames });
            var existingRoles = Database.Fetch<NodeDto>(existingSql).Select(x => x.Text);
            var missingRoles = roleNames.Except(existingRoles, StringComparer.CurrentCultureIgnoreCase);
            var missingGroups = missingRoles.Select(x => new MemberGroup {Name = x}).ToArray();

            if (AmbientScope.Events.DispatchCancelable(SavingMemberGroup, this, new SaveEventArgs<IMemberGroup>(missingGroups)))
                return;

            foreach (var m in missingGroups)
                PersistNewItem(m);

            AmbientScope.Events.Dispatch(SavedMemberGroup, this, new SaveEventArgs<IMemberGroup>(missingGroups));

            //now go get all the dto's for roles with these role names
            var rolesForNames = Database.Fetch<NodeDto>(existingSql).ToArray();

            //get the groups that are currently assigned to any of these members

            var assignedSql = Sql()
                .Select($"{SqlSyntax.GetQuotedColumnName("text")},{SqlSyntax.GetQuotedColumnName("Member")},{SqlSyntax.GetQuotedColumnName("MemberGroup")}")
                .From<NodeDto>()
                .InnerJoin<Member2MemberGroupDto>()
                .On<NodeDto, Member2MemberGroupDto>(dto => dto.NodeId, dto => dto.MemberGroup)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .WhereIn<Member2MemberGroupDto>(x => x.Member, memberIds);

            var currentlyAssigned = Database.Fetch<AssignedRolesDto>(assignedSql).ToArray();

            //assign the roles for each member id

            foreach (var memberId in memberIds)
            {
                //find any roles for the current member that are currently assigned that
                //exist in the roleNames list, then determine which ones are not currently assigned.
                var mId = memberId;
                var found = currentlyAssigned.Where(x => x.MemberId == mId).ToArray();
                var assignedRoles = found.Where(x => roleNames.Contains(x.RoleName, StringComparer.CurrentCultureIgnoreCase)).Select(x => x.RoleName);
                var nonAssignedRoles = roleNames.Except(assignedRoles, StringComparer.CurrentCultureIgnoreCase);

                foreach (var toAssign in nonAssignedRoles)
                {
                    var groupId = rolesForNames.First(x => x.Text.InvariantEquals(toAssign)).NodeId;
                    Database.Insert(new Member2MemberGroupDto { Member = mId, MemberGroup = groupId });
                }
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
            public string RoleName { get; set; }

            [Column("Member")]
            public int MemberId { get; set; }

            [Column("MemberGroup")]
            public int MemberGroupId { get; set; }
        }

        // TODO: understand why we need these two repository-level events, move them back to service

        /// <summary>
        /// Occurs before Save
        /// </summary>
        internal static event TypedEventHandler<IMemberGroupRepository, SaveEventArgs<IMemberGroup>> SavingMemberGroup;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        internal static event TypedEventHandler<IMemberGroupRepository, SaveEventArgs<IMemberGroup>> SavedMemberGroup;
    }
}
