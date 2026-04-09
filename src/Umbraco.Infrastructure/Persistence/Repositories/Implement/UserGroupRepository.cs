using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
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
    private readonly IDictionary<string, IPermissionMapper> _permissionMappers;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope.</param>
    /// <param name="appCaches">Provides application-level caching services.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="shortStringHelper">Helper for handling short string operations.</param>
    /// <param name="permissionMappers">A collection of permission mappers for user group permissions.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public UserGroupRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<UserGroupRepository> logger,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IEnumerable<IPermissionMapper> permissionMappers,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _shortStringHelper = shortStringHelper;
        _userGroupWithUsersRepository = new UserGroupWithUsersRepository(
            this,
            scopeAccessor,
            appCaches,
            loggerFactory.CreateLogger<UserGroupWithUsersRepository>(),
            repositoryCacheVersionService,
            cacheSyncService);
        _permissionRepository = new PermissionRepository<IContent>(
            scopeAccessor,
            appCaches,
            loggerFactory.CreateLogger<PermissionRepository<IContent>>(),
            repositoryCacheVersionService,
            cacheSyncService);
        _permissionMappers = permissionMappers.ToDictionary(x => x.Context);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupRepository"/> class, which manages user group persistence operations.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope.</param>
    /// <param name="appCaches">The application-level caches used for caching data.</param>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="loggerFactory">The factory used to create logger instances.</param>
    /// <param name="shortStringHelper">Helper for processing and manipulating short strings.</param>
    /// <param name="permissionMappers">A collection of permission mappers used to map permissions for user groups.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public UserGroupRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<UserGroupRepository> logger,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IEnumerable<IPermissionMapper> permissionMappers)
        : this(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            shortStringHelper,
            permissionMappers,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>(),
            StaticServiceProvider.Instance.GetRequiredService<ICacheSyncService>())
    {
    }

    /// <summary>
    /// Retrieves a user group by its alias.
    /// </summary>
    /// <param name="alias">The alias of the user group to retrieve.</param>
    /// <returns>
    /// The <see cref="IUserGroup"/> that matches the specified alias, or <c>null</c> if no such group exists.
    /// </returns>
    public IUserGroup? Get(string alias)
    {
        try
        {
            // need to do a simple query to get the id - put this cache
            var id = IsolatedCache.GetCacheItem(GetByAliasCacheKey(alias), () =>
            {
                Sql<ISqlContext> sql = Sql()
                    .Select<UserGroupDto>(x => x.Id)
                    .From<UserGroupDto>()
                    .Where<UserGroupDto>(x => x.Alias == alias);
                var groupId = Database.ExecuteScalar<int?>(sql);
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

    /// <summary>
    /// Gets the user groups that are assigned to the specified section alias.
    /// </summary>
    /// <param name="sectionAlias">The alias of the section to filter user groups by.</param>
    /// <returns>An enumerable collection of user groups assigned to the specified section.</returns>
    public IEnumerable<IUserGroup> GetGroupsAssignedToSection(string sectionAlias)
    {
        // Here we're building up a query that looks like this, a sub query is required because the resulting structure
        // needs to still contain all of the section rows per user group.
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);
        Sql<ISqlContext> innerSql = GetBaseQuery(QueryType.Ids);
        innerSql.Where<UserGroup2AppDto>(c => c.AppAlias == sectionAlias);
        sql.WhereIn<UserGroupDto>(c => c.Id, innerSql);
        AppendGroupBy(sql);

        return Database.Fetch<UserGroupDto>(sql).Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x, _permissionMappers));
    }

    /// <summary>
    /// Adds a new user group or updates an existing one, and associates the specified users with the group.
    /// </summary>
    /// <param name="userGroup">The user group to add or update.</param>
    /// <param name="userIds">An optional array of user IDs to associate with the group. If <c>null</c>, no user associations are changed.</param>
    public void AddOrUpdateGroupWithUsers(IUserGroup userGroup, int[]? userIds) =>
        _userGroupWithUsersRepository.Save(new UserGroupWithUsers(userGroup, userIds));

    /// <summary>
    ///     Gets explicitly defined permissions for the specified user groups and entities.
    /// </summary>
    /// <param name="groupIds">An array of user group IDs to retrieve permissions for.</param>
    /// <param name="entityIds">An array of entity IDs. If empty, returns permissions for the groups for all entities.</param>
    /// <returns>The collection of permissions explicitly defined for the specified groups and entities.</returns>
    public EntityPermissionCollection GetPermissions(int[] groupIds, params int[] entityIds) =>
        _permissionRepository.GetPermissionsForEntities(groupIds, entityIds);

    /// <summary>
    ///     Retrieves the permissions assigned to the specified user groups for the given entities, including explicit permissions and, if requested, default permissions when explicit ones are not set.
    /// </summary>
    /// <param name="groups">The user groups for which to retrieve permissions.</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     If <c>true</c>, includes the group's default permissions for entities where no explicit permissions are assigned.
    /// </param>
    /// <param name="nodeIds">An array of entity IDs to retrieve permissions for. If empty, permissions for all entities accessible by the group(s) are returned.</param>
    /// <returns>
    ///     An <see cref="EntityPermissionCollection"/> containing the permissions for the specified groups and entities.
    /// </returns>
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
                    var defaultPermission = new EntityPermission(group.Id, nodeId, group.Permissions ?? new HashSet<string>(), true);

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
    public void ReplaceGroupPermissions(int groupId, ISet<string> permissions, params int[] entityIds) =>
        _permissionRepository.ReplacePermissions(groupId, permissions, entityIds);

    /// <summary>
    ///     Assigns the same permission set for a single group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <param name="permission">Permissions as enumerable list of <see cref="char" /></param>
    /// <param name="entityIds">Specify the nodes to replace permissions for</param>
    public void AssignGroupPermission(int groupId, string permission, params int[] entityIds) =>
        _permissionRepository.AssignPermission(groupId, permission, entityIds);

    /// <summary>Gets the cache key for a user group by its alias.</summary>
    /// <param name="alias">The alias of the user group.</param>
    /// <returns>The cache key string for the specified user group alias.</returns>
    public static string GetByAliasCacheKey(string alias) => CacheKeys.UserGroupGetByAliasCacheKeyPrefix + alias;

    /// <summary>
    ///     used to persist a user group with associated users at once
    /// </summary>
    private sealed class UserGroupWithUsers : EntityBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupWithUsers"/> class with the specified user group and associated user IDs.
        /// </summary>
        /// <param name="userGroup">The <see cref="IUserGroup"/> instance representing the user group.</param>
        /// <param name="userIds">An array of user IDs associated with the user group, or <c>null</c> if there are no associated users.</param>
        public UserGroupWithUsers(IUserGroup userGroup, int[]? userIds)
        {
            UserGroup = userGroup;
            UserIds = userIds;
        }

        /// <summary>
        /// Gets a value indicating whether the associated user group has an identity assigned.
        /// </summary>
        public override bool HasIdentity => UserGroup.HasIdentity;

        /// <summary>
        /// Gets the associated user group for this <see cref="UserGroupWithUsers"/> instance.
        /// </summary>
        public IUserGroup UserGroup { get; }

        /// <summary>
        /// Gets the IDs of the users associated with the user group.
        /// </summary>
        public int[]? UserIds { get; }
    }

    /// <summary>
    ///     used to persist a user group with associated users at once
    /// </summary>
    private sealed class UserGroupWithUsersRepository : EntityRepositoryBase<int, UserGroupWithUsers>
    {
        private readonly UserGroupRepository _userGroupRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupWithUsersRepository"/> class.
        /// </summary>
        /// <param name="userGroupRepo">The <see cref="UserGroupRepository"/> used for user group data operations.</param>
        /// <param name="scopeAccessor">The <see cref="IScopeAccessor"/> for managing database scopes.</param>
        /// <param name="cache">The <see cref="AppCaches"/> instance for application-level caching.</param>
        /// <param name="logger">The <see cref="ILogger{UserGroupWithUsersRepository}"/> instance for logging.</param>
        /// <param name="repositoryCacheVersionService">The <see cref="IRepositoryCacheVersionService"/> for cache versioning.</param>
        /// <param name="cacheSyncService">The <see cref="ICacheSyncService"/> for synchronizing cache across servers.</param>
        public UserGroupWithUsersRepository(
            UserGroupRepository userGroupRepo,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<UserGroupWithUsersRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService) =>
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
        private void RemoveAllUsersFromGroup(int groupId)
        {
            Sql<ISqlContext> sql = Sql()
                .Delete<User2UserGroupDto>()
                .Where<User2UserGroupDto>(x => x.UserGroupId == groupId);
            Database.Execute(sql);
        }

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
        dto.UserGroup2PermissionDtos = GetUserGroupPermissions(dto.Key);
        dto.UserGroup2GranularPermissionDtos = GetUserGroupGranularPermissions(dto.Key);

        IUserGroup userGroup = UserGroupFactory.BuildEntity(_shortStringHelper, dto, _permissionMappers);
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

        AssignUserGroupOneToManyTables(ref dtos);

        return dtos.Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x, _permissionMappers));
    }

    protected override IEnumerable<IUserGroup> PerformGetByQuery(IQuery<IUserGroup> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(QueryType.Many);
        var translator = new SqlTranslator<IUserGroup>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        AppendGroupBy(sql);
        sql.OrderBy<UserGroupDto>(x => x.Id); // required for references

        List<UserGroupDto> dtos = Database.FetchOneToMany<UserGroupDto>(x => x.UserGroup2AppDtos, sql);

        AssignUserGroupOneToManyTables(ref dtos);

        return dtos.Select(x => UserGroupFactory.BuildEntity(_shortStringHelper, x, _permissionMappers));
    }

    private void AssignUserGroupOneToManyTables(ref List<UserGroupDto> userGroupDtos)
    {
        IDictionary<int, List<UserGroup2LanguageDto>> userGroups2Languages = GetAllUserGroupLanguageGrouped();
        IDictionary<Guid, List<UserGroup2PermissionDto>> userGroups2Permissions = GetAllUserGroupPermissionsGrouped();
        IDictionary<Guid, List<UserGroup2GranularPermissionDto>> userGroup2GranularPermissions = GetAllUserGroupGranularPermissionsGrouped();

        foreach (UserGroupDto dto in userGroupDtos)
        {
            userGroups2Languages.TryGetValue(dto.Id, out List<UserGroup2LanguageDto>? userGroup2LanguageDtos);
            dto.UserGroup2LanguageDtos = userGroup2LanguageDtos ?? new List<UserGroup2LanguageDto>();

            userGroups2Permissions.TryGetValue(dto.Key, out List<UserGroup2PermissionDto>? userGroup2PermissionDtos);
            dto.UserGroup2PermissionDtos = userGroup2PermissionDtos ?? new List<UserGroup2PermissionDto>();

            userGroup2GranularPermissions.TryGetValue(dto.Key, out List<UserGroup2GranularPermissionDto>? userGroup2GranularPermissionDtos);
            dto.UserGroup2GranularPermissionDtos = userGroup2GranularPermissionDtos ?? new List<UserGroup2GranularPermissionDto>();

        }
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
                sql.Select<UserGroupDto>(r => r.Select(x => x.UserGroup2AppDtos), s => s.Append($", COUNT({sql.Columns<User2UserGroupDto>(x => x.UserId)}) AS {SqlSyntax.GetQuotedName("UserCount")}"));
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
                x => x.Name,
                x => x.Description,
                x => x.HasAccessToAllLanguages,
                x => x.Key,
                x => x.DefaultPermissions)
            .AndBy<UserGroup2AppDto>(x => x.AppAlias, x => x.UserGroupId);

    protected override string GetBaseWhereClause() => $"{QuoteTableName(UserGroupDto.TableName)}.id = @id";
    protected override IEnumerable<string> GetDeleteClauses()
    {
        var userGroupId = QuoteColumnName("userGroupId");
        var userGroupKey = QuoteColumnName("userGroupKey");
        var key = QuoteColumnName(UserGroupDto.KeyColumnName);
        var umbracoUserGroup = QuoteTableName(UserGroupDto.TableName);
        var list = new List<string>
        {
            $"DELETE FROM {QuoteTableName("umbracoUser2UserGroup")} WHERE {userGroupId} = @id",
            $"DELETE FROM {QuoteTableName("umbracoUserGroup2App")} WHERE {userGroupId} = @id",
            $@"DELETE FROM {QuoteTableName("umbracoUserGroup2Permission")} WHERE {userGroupKey} IN
                (SELECT {umbracoUserGroup}.{key} FROM {umbracoUserGroup} WHERE id = @id)",
            $@"DELETE FROM {QuoteTableName("umbracoUserGroup2GranularPermission")} WHERE {userGroupKey} IN
                (SELECT {umbracoUserGroup}.{key} FROM {umbracoUserGroup} WHERE id = @id)",
            $"DELETE FROM {umbracoUserGroup} WHERE id = @id",
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
        PersistPermissions(entity);
        PersistGranularPermissions(entity);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IUserGroup entity)
    {
        entity.UpdatingEntity();

        UserGroupDto userGroupDto = UserGroupFactory.BuildDto(entity);

        Database.Update(userGroupDto);

        PersistAllowedSections(entity);
        PersistAllowedLanguages(entity);
        PersistPermissions(entity);
        PersistGranularPermissions(entity);

        entity.ResetDirtyProperties();
    }

    private void PersistAllowedSections(IUserGroup entity)
    {
        IUserGroup userGroup = entity;

        // First delete all
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2AppDto>()
            .Where<UserGroup2AppDto>(c => c.UserGroupId == userGroup.Id);
        Database.Execute(sql);

        // Then re-add any associated with the group
        foreach (var app in userGroup.AllowedSections)
        {
            var dto = new UserGroup2AppDto { UserGroupId = userGroup.Id, AppAlias = app };
            Database.Insert(dto);
        }
    }

    private void PersistAllowedLanguages(IUserGroup entity)
    {
        IUserGroup userGroup = entity;

        // First delete all
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2LanguageDto>()
            .Where<UserGroup2LanguageDto>(c => c.UserGroupId == userGroup.Id);
        Database.Execute(sql);

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

    private void PersistPermissions(IUserGroup userGroup)
    {
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2PermissionDto>()
            .Where<UserGroup2PermissionDto>(c => c.UserGroupKey == userGroup.Key);
        Database.Execute(sql);

        IEnumerable<UserGroup2PermissionDto> permissionDtos = userGroup.Permissions
            .Select(permission => new UserGroup2PermissionDto { UserGroupKey = userGroup.Key, Permission = permission });

        Database.InsertBulk(permissionDtos);
    }
    private void PersistGranularPermissions(IUserGroup userGroup)
    {
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2GranularPermissionDto>()
            .Where<UserGroup2GranularPermissionDto>(c => c.UserGroupKey == userGroup.Key);
        Database.Execute(sql);

        IEnumerable<UserGroup2GranularPermissionDto> permissionDtos = userGroup.GranularPermissions
            .Select(permission =>
            {
                var dto = new UserGroup2GranularPermissionDto
                {
                    UserGroupKey = userGroup.Key,
                    Permission = permission.Permission,
                    Context = permission.Context
                };
                if (permission is INodeGranularPermission nodeGranularPermission)
                {
                    dto.UniqueId = nodeGranularPermission.Key;
                }

                return dto;
            });

        Database.InsertBulk(permissionDtos);
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

    private List<UserGroup2PermissionDto> GetUserGroupPermissions(Guid userGroupKey)
    {
        Sql<ISqlContext> query = Sql()
            .Select<UserGroup2PermissionDto>()
            .From<UserGroup2PermissionDto>()
            .Where<UserGroup2PermissionDto>(x => x.UserGroupKey == userGroupKey);

        return Database.Fetch<UserGroup2PermissionDto>(query);
    }
    private List<UserGroup2GranularPermissionDto> GetUserGroupGranularPermissions(Guid userGroupKey)
    {
        Sql<ISqlContext> query = Sql()
            .Select<UserGroup2GranularPermissionDto>()
            .From<UserGroup2GranularPermissionDto>()
            .Where<UserGroup2GranularPermissionDto>(x => x.UserGroupKey == userGroupKey);

        return Database.Fetch<UserGroup2GranularPermissionDto>(query);
    }

    private Dictionary<Guid, List<UserGroup2PermissionDto>> GetAllUserGroupPermissionsGrouped()
    {
        Sql<ISqlContext> query = Sql()
            .Select<UserGroup2PermissionDto>()
            .From<UserGroup2PermissionDto>();

        List<UserGroup2PermissionDto> userGroupPermissions = Database.Fetch<UserGroup2PermissionDto>(query);
        return userGroupPermissions.GroupBy(x => x.UserGroupKey).ToDictionary(x => x.Key, x => x.ToList());
    }

    private Dictionary<Guid, List<UserGroup2GranularPermissionDto>> GetAllUserGroupGranularPermissionsGrouped()
    {
        Sql<ISqlContext> query = Sql()
            .Select<UserGroup2GranularPermissionDto>()
            .From<UserGroup2GranularPermissionDto>();

        List<UserGroup2GranularPermissionDto> userGroupGranularPermissions = Database.Fetch<UserGroup2GranularPermissionDto>(query);
        return userGroupGranularPermissions.GroupBy(x => x.UserGroupKey).ToDictionary(x => x.Key, x => x.ToList());
    }
    #endregion
}
