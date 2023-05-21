using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the UserGroupRepository for doing CRUD operations for <see cref="IUserGroup" />
/// </summary>
public class UserGroupRepository : EntityRepositoryBase<int, IUserGroup>, IUserGroupRepository
{
    private readonly PermissionRepository<IContent> _permissionRepository;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UserGroupWithUsersRepository _userGroupWithUsersRepository;

    public UserGroupRepository(IScopeAccessor scopeAccessor, AppCaches appCaches, ILogger<UserGroupRepository> logger, ILoggerFactory loggerFactory, IShortStringHelper shortStringHelper)
        : base(scopeAccessor, appCaches, logger)
    {
        _shortStringHelper = shortStringHelper;
        _userGroupWithUsersRepository = new UserGroupWithUsersRepository(this, scopeAccessor, appCaches, loggerFactory.CreateLogger<UserGroupWithUsersRepository>());
        _permissionRepository = new PermissionRepository<IContent>(scopeAccessor, appCaches, loggerFactory.CreateLogger<PermissionRepository<IContent>>());
    }

    public IUserGroup? Get(string alias)
    {
        try
        {
            // need to do a simple query to get the id - put this cache
            var id = IsolatedCache.GetCacheItem(GetByAliasCacheKey(alias), () =>
            {
                var groupId =
                    Database.ExecuteScalar<int?>("SELECT id FROM umbracoUserGroup WHERE userGroupAlias=@alias", new { alias });
                if (groupId.HasValue == false)
                {
                    throw new InvalidOperationException("No group found with alias " + alias);
                }

                return groupId.Value;
            });

            // return from the normal method which will cache
            return Get(id);
        }
        catch (InvalidOperationException)
        {
            // if this is caught it's because we threw this in the caching method
            return null;
        }
    }

    public IEnumerable<IUserGroup> GetGroupsAssignedToSection(string sectionAlias)
    {
        // Here we're building up a query that looks like this, a sub query is required because the resulting structure
        // needs to still contain all of the section rows per user group.

        // SELECT *
        // FROM [umbracoUserGroup]
        // LEFT JOIN [umbracoUserGroup2App]
        // ON [umbracoUserGroup].[id] = [umbracoUserGroup2App].[user]
        // WHERE umbracoUserGroup.id IN (SELECT umbracoUserGroup.id
        //    FROM [umbracoUserGroup]
        //    LEFT JOIN [umbracoUserGroup2App]
        //    ON [umbracoUserGroup].[id] = [umbracoUserGroup2App].[user]
        //    WHERE umbracoUserGroup2App.app = 'content')
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);
        Sql<ISqlContext> innerSql = GetBaseQuery(QueryType.Ids);
        innerSql.Where("umbracoUserGroup2App.app = " + SqlSyntax.GetQuotedValue(sectionAlias));
        sql.Where($"umbracoUserGroup.id IN ({innerSql.SQL})");
        AppendGroupBy(sql);

        return Database.Fetch<UserGroupDto>(sql).Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x));
    }

    public void AddOrUpdateGroupWithUsers(IUserGroup userGroup, int[]? userIds) =>
        _userGroupWithUsersRepository.Save(new UserGroupWithUsers(userGroup, userIds));

    /// <summary>
    ///     Gets explicitly defined permissions for the group for specified entities
    /// </summary>
    /// <param name="groupIds"></param>
    /// <param name="entityIds">Array of entity Ids, if empty will return permissions for the group for all entities</param>
    public EntityPermissionCollection GetPermissions(int[] groupIds, params int[] entityIds) =>
        _permissionRepository.GetPermissionsForEntities(groupIds, entityIds);

    /// <summary>
    ///     Gets explicit and default permissions (if requested) permissions for the group for specified entities
    /// </summary>
    /// <param name="groups"></param>
    /// <param name="fallbackToDefaultPermissions">
    ///     If true will include the group's default permissions if no permissions are
    ///     explicitly assigned
    /// </param>
    /// <param name="nodeIds">Array of entity Ids, if empty will return permissions for the group for all entities</param>
    public EntityPermissionCollection GetPermissions(IReadOnlyUserGroup[]? groups, bool fallbackToDefaultPermissions, params int[] nodeIds)
    {
        if (groups == null)
        {
            throw new ArgumentNullException(nameof(groups));
        }

        var groupIds = groups.Select(x => x.Id).ToArray();
        EntityPermissionCollection explicitPermissions = GetPermissions(groupIds, nodeIds);
        var result = new EntityPermissionCollection(explicitPermissions);

        // If requested, and no permissions are assigned to a particular node, then we will fill in those permissions with the group's defaults
        if (fallbackToDefaultPermissions)
        {
            // if no node ids are passed in, then we need to determine the node ids for the explicit permissions set
            nodeIds = nodeIds.Length == 0
                ? explicitPermissions.Select(x => x.EntityId).Distinct().ToArray()
                : nodeIds;

            // if there are still no nodeids we can just exit
            if (nodeIds.Length == 0)
            {
                return result;
            }

            foreach (IReadOnlyUserGroup group in groups)
            {
                foreach (var nodeId in nodeIds)
                {
                    // TODO: We could/should change the EntityPermissionsCollection into a KeyedCollection and they key could be
                    // a struct of the nodeid + groupid so then we don't actually allocate this class just to check if it's not
                    // going to be included in the result!
                    var defaultPermission = new EntityPermission(group.Id, nodeId, group.Permissions?.ToArray() ?? Array.Empty<string>(), true);

                    // Since this is a hashset, this will not add anything that already exists by group/node combination
                    result.Add(defaultPermission);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Replaces the same permission set for a single group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <param name="permissions">
    ///     Permissions as enumerable list of <see cref="char" /> If nothing is specified all permissions
    ///     are removed.
    /// </param>
    /// <param name="entityIds">Specify the nodes to replace permissions for. </param>
    public void ReplaceGroupPermissions(int groupId, IEnumerable<char>? permissions, params int[] entityIds) =>
        _permissionRepository.ReplacePermissions(groupId, permissions, entityIds);

    /// <summary>
    ///     Assigns the same permission set for a single group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <param name="permission">Permissions as enumerable list of <see cref="char" /></param>
    /// <param name="entityIds">Specify the nodes to replace permissions for</param>
    public void AssignGroupPermission(int groupId, char permission, params int[] entityIds) =>
        _permissionRepository.AssignPermission(groupId, permission, entityIds);

    public static string GetByAliasCacheKey(string alias) => CacheKeys.UserGroupGetByAliasCacheKeyPrefix + alias;

    /// <summary>
    ///     used to persist a user group with associated users at once
    /// </summary>
    private class UserGroupWithUsers : EntityBase
    {
        public UserGroupWithUsers(IUserGroup userGroup, int[]? userIds)
        {
            UserGroup = userGroup;
            UserIds = userIds;
        }

        public override bool HasIdentity => UserGroup.HasIdentity;

        public IUserGroup UserGroup { get; }

        public int[]? UserIds { get; }
    }

    /// <summary>
    ///     used to persist a user group with associated users at once
    /// </summary>
    private class UserGroupWithUsersRepository : EntityRepositoryBase<int, UserGroupWithUsers>
    {
        private readonly UserGroupRepository _userGroupRepo;

        public UserGroupWithUsersRepository(UserGroupRepository userGroupRepo, IScopeAccessor scopeAccessor, AppCaches cache, ILogger<UserGroupWithUsersRepository> logger)
            : base(scopeAccessor, cache, logger) =>
            _userGroupRepo = userGroupRepo;

        protected override void PersistNewItem(UserGroupWithUsers entity)
        {
            // save the user group
            _userGroupRepo.PersistNewItem(entity.UserGroup);

            if (entity.UserIds == null)
            {
                return;
            }

            // now the user association
            RefreshUsersInGroup(entity.UserGroup.Id, entity.UserIds);
        }

        protected override void PersistUpdatedItem(UserGroupWithUsers entity)
        {
            // save the user group
            _userGroupRepo.PersistUpdatedItem(entity.UserGroup);

            if (entity.UserIds == null)
            {
                return;
            }

            // now the user association
            RefreshUsersInGroup(entity.UserGroup.Id, entity.UserIds);
        }

        /// <summary>
        ///     Adds a set of users to a group, first removing any that exist
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="userIds">Ids of users</param>
        private void RefreshUsersInGroup(int groupId, int[] userIds)
        {
            RemoveAllUsersFromGroup(groupId);
            AddUsersToGroup(groupId, userIds);
        }

        /// <summary>
        ///     Removes all users from a group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        private void RemoveAllUsersFromGroup(int groupId) =>
            Database.Delete<User2UserGroupDto>("WHERE userGroupId = @groupId", new { groupId });

        /// <summary>
        ///     Adds a set of users to a group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="userIds">Ids of users</param>
        private void AddUsersToGroup(int groupId, int[] userIds)
        {
            foreach (var userId in userIds)
            {
                var dto = new User2UserGroupDto { UserGroupId = groupId, UserId = userId };
                Database.Insert(dto);
            }
        }

        #region Not implemented (don't need to for the purposes of this repo)

        protected override UserGroupWithUsers PerformGet(int id) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<UserGroupWithUsers> PerformGetAll(params int[]? ids) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<UserGroupWithUsers> PerformGetByQuery(IQuery<UserGroupWithUsers> query) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override string GetBaseWhereClause() =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<string> GetDeleteClauses() =>
            throw new InvalidOperationException("This method won't be implemented.");

        #endregion
    }

    #region Overrides of RepositoryBase<int,IUserGroup>

    protected override IUserGroup? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single);
        sql.Where(GetBaseWhereClause(), new { id });

        AppendGroupBy(sql);
        sql.OrderBy<UserGroupDto>(x => x.Id); // required for references

        UserGroupDto? dto = Database.FetchOneToMany<UserGroupDto>(x => x.UserGroup2AppDtos, sql).FirstOrDefault();

        if (dto == null)
        {
            return null;
        }

        dto.UserGroup2LanguageDtos = GetUserGroupLanguages(id);

        IUserGroup userGroup = UserGroupFactory.BuildEntity(_shortStringHelper, dto);
        return userGroup;
    }

    protected override IEnumerable<IUserGroup> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<UserGroupDto>(x => x.Id, ids);
        }
        else
        {
            sql.Where<UserGroupDto>(x => x.Id >= 0);
        }

        AppendGroupBy(sql);
        sql.OrderBy<UserGroupDto>(x => x.Id); // required for references

        List<UserGroupDto> dtos = Database.FetchOneToMany<UserGroupDto>(x => x.UserGroup2AppDtos, sql);

        IDictionary<int, List<UserGroup2LanguageDto>> dic = GetAllUserGroupLanguageGrouped();

        foreach (UserGroupDto dto in dtos)
        {
            dic.TryGetValue(dto.Id, out var userGroup2LanguageDtos);
            dto.UserGroup2LanguageDtos = userGroup2LanguageDtos ?? new();
        }

        return dtos.Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x));
    }

    protected override IEnumerable<IUserGroup> PerformGetByQuery(IQuery<IUserGroup> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(QueryType.Many);
        var translator = new SqlTranslator<IUserGroup>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        AppendGroupBy(sql);
        sql.OrderBy<UserGroupDto>(x => x.Id); // required for references

        List<UserGroupDto>? dtos = Database.FetchOneToMany<UserGroupDto>(x => x.UserGroup2AppDtos, sql);
        return dtos.Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x));
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,IUserGroup>

    protected Sql<ISqlContext> GetBaseQuery(QueryType type)
    {
        Sql<ISqlContext> sql = Sql();
        var addFrom = false;

        switch (type)
        {
            case QueryType.Count:
                sql
                    .SelectCount()
                    .From<UserGroupDto>();
                break;
            case QueryType.Ids:
                sql
                    .Select<UserGroupDto>(x => x.Id);
                addFrom = true;
                break;
            case QueryType.Single:
            case QueryType.Many:
                sql.Select<UserGroupDto>(r => r.Select(x => x.UserGroup2AppDtos), s => s.Append($", COUNT({sql.Columns<User2UserGroupDto>(x => x.UserId)}) AS {SqlSyntax.GetQuotedColumnName("UserCount")}"));
                addFrom = true;
                break;
            default:
                throw new NotSupportedException(type.ToString());
        }

        if (addFrom)
        {
            sql
                .From<UserGroupDto>()
                .LeftJoin<UserGroup2AppDto>()
                .On<UserGroupDto, UserGroup2AppDto>(left => left.Id, right => right.UserGroupId)
                .LeftJoin<User2UserGroupDto>()
                .On<User2UserGroupDto, UserGroupDto>(left => left.UserGroupId, right => right.Id);
        }

        return sql;
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        GetBaseQuery(isCount ? QueryType.Count : QueryType.Many);

    private static void AppendGroupBy(Sql<ISqlContext> sql) =>
        sql.GroupBy<UserGroupDto>(
                x => x.CreateDate,
                x => x.Icon,
                x => x.Id,
                x => x.StartContentId,
                x => x.StartMediaId,
                x => x.UpdateDate,
                x => x.Alias,
                x => x.DefaultPermissions,
                x => x.Name,
                x => x.HasAccessToAllLanguages)
            .AndBy<UserGroup2AppDto>(x => x.AppAlias, x => x.UserGroupId);

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.UserGroup}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM umbracoUser2UserGroup WHERE userGroupId = @id",
            "DELETE FROM umbracoUserGroup2App WHERE userGroupId = @id",
            "DELETE FROM umbracoUserGroup2Node WHERE userGroupId = @id",
            "DELETE FROM umbracoUserGroup2NodePermission WHERE userGroupId = @id",
            "DELETE FROM umbracoUserGroup WHERE id = @id",
        };
        return list;
    }

    protected override void PersistNewItem(IUserGroup entity)
    {
        entity.AddingEntity();

        UserGroupDto userGroupDto = UserGroupFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(userGroupDto));
        entity.Id = id;

        PersistAllowedSections(entity);
        PersistAllowedLanguages(entity);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IUserGroup entity)
    {
        entity.UpdatingEntity();

        UserGroupDto userGroupDto = UserGroupFactory.BuildDto(entity);

        Database.Update(userGroupDto);

        PersistAllowedSections(entity);
            PersistAllowedLanguages(entity);

        entity.ResetDirtyProperties();
    }

    private void PersistAllowedSections(IUserGroup entity)
    {
        IUserGroup userGroup = entity;

        // First delete all
        Database.Delete<UserGroup2AppDto>("WHERE UserGroupId = @UserGroupId", new { UserGroupId = userGroup.Id });

        // Then re-add any associated with the group
        foreach (var app in userGroup.AllowedSections)
        {
            var dto = new UserGroup2AppDto { UserGroupId = userGroup.Id, AppAlias = app };
            Database.Insert(dto);
        }
    }

        private void PersistAllowedLanguages(IUserGroup entity)
        {
            var userGroup = entity;

            // First delete all
            Database.Delete<UserGroup2LanguageDto>("WHERE UserGroupId = @UserGroupId", new { UserGroupId = userGroup.Id });

            // Then re-add any associated with the group
            foreach (var language in userGroup.AllowedLanguages)
            {
                var dto = new UserGroup2LanguageDto
                {
                    UserGroupId = userGroup.Id,
                    LanguageId = language,
                };

                Database.Insert(dto);
            }
        }

        private List<UserGroup2LanguageDto> GetUserGroupLanguages(int userGroupId)
        {
            Sql<ISqlContext> query = Sql()
                .Select<UserGroup2LanguageDto>()
                .From<UserGroup2LanguageDto>()
                .Where<UserGroup2LanguageDto>(x => x.UserGroupId == userGroupId);
            return Database.Fetch<UserGroup2LanguageDto>(query);
        }

        private IDictionary<int, List<UserGroup2LanguageDto>> GetAllUserGroupLanguageGrouped()
        {
            Sql<ISqlContext> query = Sql()
                .Select<UserGroup2LanguageDto>()
                .From<UserGroup2LanguageDto>();
            List<UserGroup2LanguageDto> userGroupLanguages = Database.Fetch<UserGroup2LanguageDto>(query);
            return userGroupLanguages.GroupBy(x => x.UserGroupId).ToDictionary(x => x.Key, x => x.ToList());
        }

    #endregion
}
