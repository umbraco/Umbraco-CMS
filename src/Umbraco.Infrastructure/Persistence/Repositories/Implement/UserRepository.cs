using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
/// <summary>
/// Represents the UserRepository for doing CRUD operations for <see cref="IUser"/>
/// </summary>
internal class UserRepository : EntityRepositoryBase<int, IUser>, IUserRepository
{
    private readonly IMapperCollection _mapperCollection;
    private readonly GlobalSettings _globalSettings;
    private readonly UserPasswordConfigurationSettings _passwordConfiguration;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IRuntimeState _runtimeState;
    private string? _passwordConfigJson;
    private bool _passwordConfigInitialized;
    private readonly object _sqliteValidateSessionLock = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRepository" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mapperCollection">
    ///     A dictionary specifying the configuration for user passwords. If this is null then no
    ///     password configuration will be persisted or read.
    /// </param>
    /// <param name="globalSettings">The global settings.</param>
    /// <param name="passwordConfiguration">The password configuration.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="runtimeState">State of the runtime.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     mapperCollection
    ///     or
    ///     globalSettings
    ///     or
    ///     passwordConfiguration
    /// </exception>
    public UserRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<UserRepository> logger,
        IMapperCollection mapperCollection,
        IOptions<GlobalSettings> globalSettings,
        IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
        IJsonSerializer jsonSerializer,
        IRuntimeState runtimeState)
        : base(scopeAccessor, appCaches, logger)
    {
        _mapperCollection = mapperCollection ?? throw new ArgumentNullException(nameof(mapperCollection));
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        _passwordConfiguration =
            passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
        _jsonSerializer = jsonSerializer;
        _runtimeState = runtimeState;
    }

    /// <summary>
    ///     Returns a serialized dictionary of the password configuration that is stored against the user in the database
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

    private IEnumerable<IUser> ConvertFromDtos(IEnumerable<UserDto> dtos) =>
        dtos.Select(x => UserFactory.BuildEntity(_globalSettings, x));

    #region Overrides of RepositoryBase<int,IUser>

    protected override IUser? PerformGet(int id)
    {
        // This will never resolve to a user, yet this is asked
        // for all of the time (especially in cases of members).
        // Don't issue a SQL call for this, we know it will not exist.
        if (_runtimeState.Level == RuntimeLevel.Upgrade)
        {
            // when upgrading people might come from version 7 where user 0 was the default,
            // only in upgrade mode do we want to fetch the user of Id 0
            if (id < -1)
            {
                return null;
            }
        }
        else
        {
            if (id == default || id < -1)
            {
                return null;
            }
        }

        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<UserDto>()
            .From<UserDto>()
            .Where<UserDto>(x => x.Id == id);

        List<UserDto>? dtos = Database.Fetch<UserDto>(sql);
        if (dtos.Count == 0)
        {
            return null;
        }

        PerformGetReferencedDtos(dtos);
        return UserFactory.BuildEntity(_globalSettings, dtos[0]);
    }

    /// <summary>
    ///     Returns a user by username
    /// </summary>
    /// <param name="username"></param>
    /// <param name="includeSecurityData">
    ///     Can be used for slightly faster user lookups if the result doesn't require security data (i.e. groups, apps & start nodes).
    ///     This is really only used for a shim in order to upgrade to 7.6.
    /// </param>
    /// <returns>
    ///     A non cached <see cref="IUser" /> instance
    /// </returns>
    public IUser? GetByUsername(string username, bool includeSecurityData) =>
        GetWith(sql => sql.Where<UserDto>(x => x.Login == username), includeSecurityData);

    /// <summary>
    ///     Returns a user by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeSecurityData">
    ///     This is really only used for a shim in order to upgrade to 7.6 but could be used
    ///     for slightly faster user lookups if the result doesn't require security data (i.e. groups, apps & start nodes)
    /// </param>
    /// <returns>
    ///     A non cached <see cref="IUser"/> instance
    /// </returns>
    public IUser? Get(int? id, bool includeSecurityData) =>
        GetWith(sql => sql.Where<UserDto>(x => x.Id == id), includeSecurityData);

    public IProfile? GetProfile(string username)
    {
        UserDto? dto = GetDtoWith(sql => sql.Where<UserDto>(x => x.Login == username), false);
        return dto == null ? null : new UserProfile(dto.Id, dto.UserName);
    }

    public IProfile? GetProfile(int id)
    {
        UserDto? dto = GetDtoWith(sql => sql.Where<UserDto>(x => x.Id == id), false);
        return dto == null ? null : new UserProfile(dto.Id, dto.UserName);
    }

    public IDictionary<UserState, int> GetUserStates()
    {
        // These keys in this query map to the `Umbraco.Core.Models.Membership.UserState` enum
        var sql = @"SELECT -1 AS [Key], COUNT(id) AS [Value] FROM umbracoUser
UNION
SELECT 0 AS [Key], COUNT(id) AS [Value] FROM umbracoUser WHERE userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NOT NULL
UNION
SELECT 1 AS [Key], COUNT(id) AS [Value] FROM umbracoUser WHERE userDisabled = 1
UNION
SELECT 2 AS [Key], COUNT(id) AS [Value] FROM umbracoUser WHERE userNoConsole = 1
UNION
SELECT 3 AS [Key], COUNT(id) AS [Value] FROM umbracoUser WHERE lastLoginDate IS NULL AND userDisabled = 1 AND invitedDate IS NOT NULL
UNION
SELECT 4 AS [Key], COUNT(id) AS [Value] FROM umbracoUser WHERE userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NULL";

        Dictionary<int, int>? result = Database.Dictionary<int, int>(sql);

        return result.ToDictionary(x => (UserState)x.Key, x => x.Value);
    }

    public Guid CreateLoginSession(int? userId, string requestingIpAddress, bool cleanStaleSessions = true)
    {
        DateTime now = DateTime.UtcNow;
        var dto = new UserLoginDto
        {
            UserId = userId,
            IpAddress = requestingIpAddress,
            LoggedInUtc = now,
            LastValidatedUtc = now,
            LoggedOutUtc = null,
            SessionId = Guid.NewGuid()
        };
        Database.Insert(dto);

        if (cleanStaleSessions)
        {
            ClearLoginSessions(TimeSpan.FromDays(15));
        }

        return dto.SessionId;
    }

    public bool ValidateLoginSession(int userId, Guid sessionId)
    {
        // HACK: Avoid a deadlock - BackOfficeCookieOptions OnValidatePrincipal
            // After existing session times out and user logs in again ~ 4 requests come in at once that hit the
            // "update the validate date" code path, check up the call stack there are a few variables that can make this not occur.
            // TODO: more generic fix, do something with ForUpdate? wait on a mutex? add a distributed lock? etc.
            if (Database.DatabaseType.IsSqlite())
            {
                lock (_sqliteValidateSessionLock)
                {
                    return ValidateLoginSessionInternal(userId, sessionId);
                }
            }

            return ValidateLoginSessionInternal(userId, sessionId);
        }

        private bool ValidateLoginSessionInternal(int userId, Guid sessionId)
        {
            // with RepeatableRead transaction mode, read-then-update operations can
        // cause deadlocks, and the ForUpdate() hint is required to tell the database
        // to acquire an exclusive lock when reading

        // that query is going to run a *lot*, make it a template
        SqlTemplate t = SqlContext.Templates.Get("Umbraco.Core.UserRepository.ValidateLoginSession", s => s
            .Select<UserLoginDto>()
            .From<UserLoginDto>()
            .Where<UserLoginDto>(x => x.SessionId == SqlTemplate.Arg<Guid>("sessionId"))
            .ForUpdate()
            .SelectTop(1)); // Stick at end, SQL server syntax provider will insert at start of query after "select ", but sqlite will append limit to end.

        Sql<ISqlContext> sql = t.Sql(sessionId);

        UserLoginDto? found = Database.FirstOrDefault<UserLoginDto>(sql);
        if (found == null || found.UserId != userId || found.LoggedOutUtc.HasValue)
        {
            return false;
        }

        //now detect if there's been a timeout
        if (DateTime.UtcNow - found.LastValidatedUtc > _globalSettings.TimeOut)
        {
            //timeout detected, update the record
            Logger.LogDebug("ClearLoginSession for sessionId {sessionId}", sessionId);ClearLoginSession(sessionId);
            return false;
        }

        //update the validate date
        Logger.LogDebug("Updating LastValidatedUtc for sessionId {sessionId}", sessionId);found.LastValidatedUtc = DateTime.UtcNow;
        Database.Update(found);
        return true;
    }

    public int ClearLoginSessions(int userId) =>
        Database.Delete<UserLoginDto>(Sql().Where<UserLoginDto>(x => x.UserId == userId));

    public int ClearLoginSessions(TimeSpan timespan)
    {
        DateTime fromDate = DateTime.UtcNow - timespan;
        return Database.Delete<UserLoginDto>(Sql().Where<UserLoginDto>(x => x.LastValidatedUtc < fromDate));
    }

    public void ClearLoginSession(Guid sessionId) =>
        // TODO: why is that one updating and not deleting?
        Database.Execute(Sql()
            .Update<UserLoginDto>(u => u.Set(x => x.LoggedOutUtc, DateTime.UtcNow))
            .Where<UserLoginDto>(x => x.SessionId == sessionId));

    protected override IEnumerable<IUser> PerformGetAll(params int[]? ids)
    {
        List<UserDto> dtos = ids?.Length == 0
            ? GetDtosWith(null, true)
            : GetDtosWith(sql => sql.WhereIn<UserDto>(x => x.Id, ids), true);
        var users = new IUser[dtos.Count];
        var i = 0;
        foreach (UserDto dto in dtos)
        {
            users[i++] = UserFactory.BuildEntity(_globalSettings, dto);
        }

        return users;
    }

    protected override IEnumerable<IUser> PerformGetByQuery(IQuery<IUser> query)
    {
        var dtos = GetDtosWith(sql => new SqlTranslator<IUser>(sql, query).Translate(), true)
            .DistinctBy(x => x.Id)
            .ToList();

        var users = new IUser[dtos.Count];
        var i = 0;
        foreach (UserDto dto in dtos)
        {
            users[i++] = UserFactory.BuildEntity(_globalSettings, dto);
        }

        return users;
    }

    private IUser? GetWith(Action<Sql<ISqlContext>> with, bool includeReferences)
    {
        UserDto? dto = GetDtoWith(with, includeReferences);
        return dto == null ? null : UserFactory.BuildEntity(_globalSettings, dto);
    }

    private UserDto? GetDtoWith(Action<Sql<ISqlContext>> with, bool includeReferences)
    {
        List<UserDto> dtos = GetDtosWith(with, includeReferences);
        return dtos.FirstOrDefault();
    }

    private List<UserDto> GetDtosWith(Action<Sql<ISqlContext>>? with, bool includeReferences)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<UserDto>()
            .From<UserDto>();

        with?.Invoke(sql);

        List<UserDto>? dtos = Database.Fetch<UserDto>(sql);

        if (includeReferences)
        {
            PerformGetReferencedDtos(dtos);
        }

        return dtos;
    }

    // NPoco cannot fetch 2+ references at a time
    // plus it creates a combinatorial explosion
    // better use extra queries
    // unfortunately, SqlCe doesn't support multiple result sets
    private void PerformGetReferencedDtos(List<UserDto> dtos)
    {
        if (dtos.Count == 0)
        {
            return;
        }

        List<int> userIds = dtos.Count == 1 ? new List<int> {dtos[0].Id} : dtos.Select(x => x.Id).ToList();
        Dictionary<int, UserDto>? xUsers = dtos.Count == 1 ? null : dtos.ToDictionary(x => x.Id, x => x);

        // get users2groups

        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<User2UserGroupDto>()
            .From<User2UserGroupDto>()
            .WhereIn<User2UserGroupDto>(x => x.UserId, userIds);

        List<User2UserGroupDto>? user2Groups = Database.Fetch<User2UserGroupDto>(sql);
        var groupIds = user2Groups.Select(x => x.UserGroupId).ToList();

        // get groups
        // We wrap this in a try-catch, as this might throw errors when you try to login before having migrated your database
        Dictionary<int, UserGroupDto> groups;
        try
        {
            sql = SqlContext.Sql()
                .Select<UserGroupDto>()
                .From<UserGroupDto>()
                .WhereIn<UserGroupDto>(x => x.Id, groupIds);

            groups = Database.Fetch<UserGroupDto>(sql)
                .ToDictionary(x => x.Id, x => x);
        }
        catch(Exception e)
        {
            Logger.LogDebug(e, "Couldn't get user groups. This should only happens doing the migration that add new columns to user groups");

            sql = SqlContext.Sql()
                .Select<UserGroupDto>(x=>x.Id, x=>x.Alias, x=>x.StartContentId, x=>x.StartMediaId)
                .From<UserGroupDto>()
                .WhereIn<UserGroupDto>(x => x.Id, groupIds);

            groups = Database.Fetch<UserGroupDto>(sql)
                .ToDictionary(x => x.Id, x => x);
        }

        // get groups2apps

        sql = SqlContext.Sql()
            .Select<UserGroup2AppDto>()
            .From<UserGroup2AppDto>()
            .WhereIn<UserGroup2AppDto>(x => x.UserGroupId, groupIds);

        var groups2Apps = Database.Fetch<UserGroup2AppDto>(sql)
            .GroupBy(x => x.UserGroupId)
            .ToDictionary(x => x.Key, x => x);

        // get start nodes

        sql = SqlContext.Sql()
            .Select<UserStartNodeDto>()
            .From<UserStartNodeDto>()
            .WhereIn<UserStartNodeDto>(x => x.UserId, userIds);

        List<UserStartNodeDto>? startNodes = Database.Fetch<UserStartNodeDto>(sql);

        // get groups2languages

        sql = SqlContext.Sql()
            .Select<UserGroup2LanguageDto>()
            .From<UserGroup2LanguageDto>()
            .WhereIn<UserGroup2LanguageDto>(x => x.UserGroupId, groupIds);

        Dictionary<int, IGrouping<int, UserGroup2LanguageDto>> groups2languages;
        try
        {
            groups2languages = Database.Fetch<UserGroup2LanguageDto>(sql)
                .GroupBy(x => x.UserGroupId)
                .ToDictionary(x => x.Key, x => x);
        }
        catch
        {
            // If we get an error, the table has not been made in the database yet, set the list to an empty one
            groups2languages = new Dictionary<int, IGrouping<int, UserGroup2LanguageDto>>();
        }

        // map groups

        foreach (User2UserGroupDto? user2Group in user2Groups)
        {
            if (groups.TryGetValue(user2Group.UserGroupId, out UserGroupDto? group))
            {
                UserDto dto = xUsers == null ? dtos[0] : xUsers[user2Group.UserId];
                dto.UserGroupDtos.Add(group); // user2group is distinct
            }
        }

        // map start nodes

        foreach (UserStartNodeDto? startNode in startNodes)
        {
            UserDto dto = xUsers == null ? dtos[0] : xUsers[startNode.UserId];
            dto.UserStartNodeDtos.Add(startNode); // hashset = distinct
        }

        // map apps

        foreach (UserGroupDto? group in groups.Values)
        {
            if (groups2Apps.TryGetValue(group.Id, out IGrouping<int, UserGroup2AppDto>? list))
            {
                group.UserGroup2AppDtos = list.ToList(); // groups2apps is distinct
            }

        }

        // map languages

        foreach (var group in groups.Values)
        {
            if (groups2languages.TryGetValue(group.Id, out var list))
            {
                group.UserGroup2LanguageDtos = list.ToList(); // groups2apps is distinct
            }
        }
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,IUser>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        if (isCount)
        {
            return SqlContext.Sql()
                .SelectCount()
                .From<UserDto>();
        }

        return SqlContext.Sql()
            .Select<UserDto>()
            .From<UserDto>();
    }

    private static void AddGroupLeftJoin(Sql<ISqlContext> sql) =>
        sql
            .LeftJoin<User2UserGroupDto>()
            .On<User2UserGroupDto, UserDto>(left => left.UserId, right => right.Id)
            .LeftJoin<UserGroupDto>()
            .On<UserGroupDto, User2UserGroupDto>(left => left.Id, right => right.UserGroupId)
            .LeftJoin<UserGroup2AppDto>()
            .On<UserGroup2AppDto, UserGroupDto>(left => left.UserGroupId, right => right.Id)
            .LeftJoin<UserStartNodeDto>()
            .On<UserStartNodeDto, UserDto>(left => left.UserId, right => right.Id);

    private Sql<ISqlContext> GetBaseQuery(string columns) =>
        SqlContext.Sql()
            .Select(columns)
            .From<UserDto>();

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.User}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            $"DELETE FROM {Constants.DatabaseSchema.Tables.UserLogin} WHERE userId = @id",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.User2UserGroup} WHERE userId = @id",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.User2NodeNotify} WHERE userId = @id",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.UserStartNode} WHERE userId = @id",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.ExternalLoginToken} WHERE externalLoginId = (SELECT id FROM {Constants.DatabaseSchema.Tables.ExternalLogin} WHERE userOrMemberKey = @key)",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.ExternalLogin} WHERE userOrMemberKey = @key",
            $"DELETE FROM {Constants.DatabaseSchema.Tables.User} WHERE id = @id",
        };
        return list;
    }
    protected override void PersistDeletedItem(IUser entity)
    {
        IEnumerable<string> deletes = GetDeleteClauses();
        foreach (var delete in deletes)
        {
            Database.Execute(delete, new { id = GetEntityId(entity), key = entity.Key });
        }

        entity.DeleteDate = DateTime.Now;
    }

    protected override void PersistNewItem(IUser entity)
    {
        entity.AddingEntity();

        // ensure security stamp if missing
        if (entity.SecurityStamp.IsNullOrWhiteSpace())
        {
            entity.SecurityStamp = Guid.NewGuid().ToString();
        }

        UserDto userDto = UserFactory.BuildDto(entity);

        // check if we have a user config else use the default
        userDto.PasswordConfig = entity.PasswordConfiguration ?? DefaultPasswordConfigJson;

        var id = Convert.ToInt32(Database.Insert(userDto));
        entity.Id = id;

        if (entity.IsPropertyDirty("StartContentIds"))
        {
            AddingOrUpdateStartNodes(entity, Enumerable.Empty<UserStartNodeDto>(),
                UserStartNodeDto.StartNodeTypeValue.Content, entity.StartContentIds);
        }

        if (entity.IsPropertyDirty("StartMediaIds"))
        {
            AddingOrUpdateStartNodes(entity, Enumerable.Empty<UserStartNodeDto>(),
                UserStartNodeDto.StartNodeTypeValue.Media, entity.StartMediaIds);
        }

        if (entity.IsPropertyDirty("Groups"))
        {
            // lookup all assigned
            List<UserGroupDto>? assigned = entity.Groups == null || entity.Groups.Any() == false
                ? new List<UserGroupDto>()
                : Database.Fetch<UserGroupDto>("SELECT * FROM umbracoUserGroup WHERE userGroupAlias IN (@aliases)",
                    new {aliases = entity.Groups.Select(x => x.Alias)});

            foreach (UserGroupDto? groupDto in assigned)
            {
                var dto = new User2UserGroupDto {UserGroupId = groupDto.Id, UserId = entity.Id};
                Database.Insert(dto);
            }
        }

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IUser entity)
    {
        // updates Modified date
        entity.UpdatingEntity();

        // ensure security stamp if missing
        if (entity.SecurityStamp.IsNullOrWhiteSpace())
        {
            entity.SecurityStamp = Guid.NewGuid().ToString();
        }

        UserDto userDto = UserFactory.BuildDto(entity);

        // build list of columns to check for saving - we don't want to save the password if it hasn't changed!
        // list the columns to save, NOTE: would be nice to not have hard coded strings here but no real good way around that
        var colsToSave = new Dictionary<string, string>
        {
            //TODO: Change these to constants + nameof
            {"userDisabled", "IsApproved"},
            {"userNoConsole", "IsLockedOut"},
            {"startStructureID", "StartContentId"},
            {"startMediaID", "StartMediaId"},
            {"userName", "Name"},
            {"userLogin", "Username"},
            {"userEmail", "Email"},
            {"userLanguage", "Language"},
            {"securityStampToken", "SecurityStamp"},
            {"lastLockoutDate", "LastLockoutDate"},
            {"lastPasswordChangeDate", "LastPasswordChangeDate"},
            {"lastLoginDate", "LastLoginDate"},
            {"failedLoginAttempts", "FailedPasswordAttempts"},
            {"createDate", "CreateDate"},
            {"updateDate", "UpdateDate"},
            {"avatar", "Avatar"},
            {"emailConfirmedDate", "EmailConfirmedDate"},
            {"invitedDate", "InvitedDate"},
            {"tourData", "TourData"}
        };

        // create list of properties that have changed
        var changedCols = colsToSave
            .Where(col => entity.IsPropertyDirty(col.Value))
            .Select(col => col.Key)
            .ToList();

        if (entity.IsPropertyDirty("SecurityStamp"))
        {
            changedCols.Add("securityStampToken");
        }

        // DO NOT update the password if it has not changed or if it is null or empty
        if (entity.IsPropertyDirty("RawPasswordValue") && entity.RawPasswordValue.IsNullOrWhiteSpace() == false)
        {
            changedCols.Add("userPassword");

            // If the security stamp hasn't already updated we need to force it
            if (entity.IsPropertyDirty("SecurityStamp") == false)
            {
                userDto.SecurityStampToken = entity.SecurityStamp = Guid.NewGuid().ToString();
                changedCols.Add("securityStampToken");
            }

            // check if we have a user config else use the default
            userDto.PasswordConfig = entity.PasswordConfiguration ?? DefaultPasswordConfigJson;
            changedCols.Add("passwordConfig");
        }

        // If userlogin or the email has changed then need to reset security stamp
        if (changedCols.Contains("userLogin") || changedCols.Contains("userEmail"))
        {
            userDto.EmailConfirmedDate = null;
            changedCols.Add("emailConfirmedDate");

            // If the security stamp hasn't already updated we need to force it
            if (entity.IsPropertyDirty("SecurityStamp") == false)
            {
                userDto.SecurityStampToken = entity.SecurityStamp = Guid.NewGuid().ToString();
                changedCols.Add("securityStampToken");
            }
        }

        //only update the changed cols
        if (changedCols.Count > 0)
        {
            Database.Update(userDto, changedCols);
        }

        if (entity.IsPropertyDirty("StartContentIds") || entity.IsPropertyDirty("StartMediaIds"))
        {
            List<UserStartNodeDto>? assignedStartNodes =
                Database.Fetch<UserStartNodeDto>("SELECT * FROM umbracoUserStartNode WHERE userId = @userId",
                    new {userId = entity.Id});
            if (entity.IsPropertyDirty("StartContentIds"))
            {
                AddingOrUpdateStartNodes(entity, assignedStartNodes, UserStartNodeDto.StartNodeTypeValue.Content,
                    entity.StartContentIds);
            }

            if (entity.IsPropertyDirty("StartMediaIds"))
            {
                AddingOrUpdateStartNodes(entity, assignedStartNodes, UserStartNodeDto.StartNodeTypeValue.Media,
                    entity.StartMediaIds);
            }
        }

        if (entity.IsPropertyDirty("Groups"))
        {
            //lookup all assigned
            List<UserGroupDto>? assigned = entity.Groups == null || entity.Groups.Any() == false
                ? new List<UserGroupDto>()
                : Database.Fetch<UserGroupDto>("SELECT * FROM umbracoUserGroup WHERE userGroupAlias IN (@aliases)",
                    new {aliases = entity.Groups.Select(x => x.Alias)});

            //first delete all
            // TODO: We could do this a nicer way instead of "Nuke and Pave"
            Database.Delete<User2UserGroupDto>("WHERE UserId = @UserId", new {UserId = entity.Id});

            foreach (UserGroupDto? groupDto in assigned)
            {
                var dto = new User2UserGroupDto {UserGroupId = groupDto.Id, UserId = entity.Id};
                Database.Insert(dto);
            }
        }

        entity.ResetDirtyProperties();
    }

    private void AddingOrUpdateStartNodes(IEntity entity, IEnumerable<UserStartNodeDto> current,
        UserStartNodeDto.StartNodeTypeValue startNodeType, int[]? entityStartIds)
    {
        if (entityStartIds is null)
        {
            return;
        }

        var assignedIds = current.Where(x => x.StartNodeType == (int)startNodeType).Select(x => x.StartNode).ToArray();

        //remove the ones not assigned to the entity
        var toDelete = assignedIds.Except(entityStartIds).ToArray();
        if (toDelete.Length > 0)
        {
            Database.Delete<UserStartNodeDto>("WHERE UserId = @UserId AND startNode IN (@startNodes)",
                new {UserId = entity.Id, startNodes = toDelete});
        }

        //add the ones not currently in the db
        var toAdd = entityStartIds.Except(assignedIds).ToArray();
        foreach (var i in toAdd)
        {
            var dto = new UserStartNodeDto {StartNode = i, StartNodeType = (int)startNodeType, UserId = entity.Id};
            Database.Insert(dto);
        }
    }

    #endregion

    #region Implementation of IUserRepository

    public int GetCountByQuery(IQuery<IUser>? query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery("umbracoUser.id");
        var translator = new SqlTranslator<IUser>(sqlClause, query);
        Sql<ISqlContext> subquery = translator.Translate();
        //get the COUNT base query
        Sql<ISqlContext>? sql = GetBaseQuery(true)
            .Append(new Sql("WHERE umbracoUser.id IN (" + subquery.SQL + ")", subquery.Arguments));

        return Database.ExecuteScalar<int>(sql);
    }

    public bool Exists(string username) => ExistsByUserName(username);

    public bool ExistsByUserName(string username)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .SelectCount()
            .From<UserDto>()
            .Where<UserDto>(x => x.UserName == username);

        return Database.ExecuteScalar<int>(sql) > 0;
    }

    public bool ExistsByLogin(string login)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .SelectCount()
            .From<UserDto>()
            .Where<UserDto>(x => x.Login == login);

        return Database.ExecuteScalar<int>(sql) > 0;
    }

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    public IEnumerable<IUser> GetAllInGroup(int groupId) => GetAllInOrNotInGroup(groupId, true);

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects not associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    public IEnumerable<IUser> GetAllNotInGroup(int groupId) => GetAllInOrNotInGroup(groupId, false);

    private IEnumerable<IUser> GetAllInOrNotInGroup(int groupId, bool include)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<UserDto>()
            .From<UserDto>();

        Sql<ISqlContext> inSql = SqlContext.Sql()
            .Select<User2UserGroupDto>(x => x.UserId)
            .From<User2UserGroupDto>()
            .Where<User2UserGroupDto>(x => x.UserGroupId == groupId);

        if (include)
        {
            sql.WhereIn<UserDto>(x => x.Id, inSql);
        }
        else
        {
            sql.WhereNotIn<UserDto>(x => x.Id, inSql);
        }


        List<UserDto>? dtos = Database.Fetch<UserDto>(sql);

        //adds missing bits like content and media start nodes
        PerformGetReferencedDtos(dtos);

        return ConvertFromDtos(dtos);
    }

    /// <summary>
    ///     Gets paged user results
    /// </summary>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="includeUserGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="excludeUserGroups">
    ///     A filter to only include users that do not belong to these user groups
    /// </param>
    /// <param name="userState">Optional parameter to filter by specified user state</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <remarks>
    ///     The query supplied will ONLY work with data specifically on the umbracoUser table because we are using NPoco paging
    ///     (SQL paging)
    /// </remarks>
    public IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser>? query, long pageIndex, int pageSize,
        out long totalRecords,
        Expression<Func<IUser, object?>> orderBy, Direction orderDirection = Direction.Ascending,
        string[]? includeUserGroups = null, string[]? excludeUserGroups = null, UserState[]? userState = null,
        IQuery<IUser>? filter = null)
    {
        if (orderBy == null)
        {
            throw new ArgumentNullException(nameof(orderBy));
        }

        Sql<ISqlContext>? filterSql = null;
        Tuple<string, object[]>[]? customFilterWheres = filter?.GetWhereClauses().ToArray();
        var hasCustomFilter = customFilterWheres != null && customFilterWheres.Length > 0;
        if (hasCustomFilter
            || (includeUserGroups != null && includeUserGroups.Length > 0)
            || (excludeUserGroups != null && excludeUserGroups.Length > 0)
            || (userState != null && userState.Length > 0 && userState.Contains(UserState.All) == false))
        {
            filterSql = SqlContext.Sql();
        }

        if (hasCustomFilter)
        {
            foreach (Tuple<string, object[]> clause in customFilterWheres!)
            {
                filterSql?.Append($"AND ({clause.Item1})", clause.Item2);
            }
        }

        if (includeUserGroups != null && includeUserGroups.Length > 0)
        {
            const string subQuery = @"AND (umbracoUser.id IN (SELECT DISTINCT umbracoUser.id
                    FROM umbracoUser
                    INNER JOIN umbracoUser2UserGroup ON umbracoUser2UserGroup.userId = umbracoUser.id
                    INNER JOIN umbracoUserGroup ON umbracoUserGroup.id = umbracoUser2UserGroup.userGroupId
                    WHERE umbracoUserGroup.userGroupAlias IN (@userGroups)))";
            filterSql?.Append(subQuery, new {userGroups = includeUserGroups});
        }

        if (excludeUserGroups != null && excludeUserGroups.Length > 0)
        {
            const string subQuery = @"AND (umbracoUser.id NOT IN (SELECT DISTINCT umbracoUser.id
                    FROM umbracoUser
                    INNER JOIN umbracoUser2UserGroup ON umbracoUser2UserGroup.userId = umbracoUser.id
                    INNER JOIN umbracoUserGroup ON umbracoUserGroup.id = umbracoUser2UserGroup.userGroupId
                    WHERE umbracoUserGroup.userGroupAlias IN (@userGroups)))";
            filterSql?.Append(subQuery, new {userGroups = excludeUserGroups});
        }

        if (userState != null && userState.Length > 0)
        {
            //the "ALL" state doesn't require any filtering so we ignore that, if it exists in the list we don't do any filtering
            if (userState.Contains(UserState.All) == false)
            {
                var sb = new StringBuilder("(");
                var appended = false;

                if (userState.Contains(UserState.Active))
                {
                    sb.Append("(userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NOT NULL)");
                    appended = true;
                }

                if (userState.Contains(UserState.Inactive))
                {
                    if (appended)
                    {
                        sb.Append(" OR ");
                    }

                    sb.Append("(userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NULL)");
                    appended = true;
                }

                if (userState.Contains(UserState.Disabled))
                {
                    if (appended)
                    {
                        sb.Append(" OR ");
                    }

                    sb.Append("(userDisabled = 1)");
                    appended = true;
                }

                if (userState.Contains(UserState.LockedOut))
                {
                    if (appended)
                    {
                        sb.Append(" OR ");
                    }

                    sb.Append("(userNoConsole = 1)");
                    appended = true;
                }

                if (userState.Contains(UserState.Invited))
                {
                    if (appended)
                    {
                        sb.Append(" OR ");
                    }

                    sb.Append("(lastLoginDate IS NULL AND userDisabled = 1 AND invitedDate IS NOT NULL)");
                    appended = true;
                }

                sb.Append(")");
                filterSql?.Append("AND " + sb);
            }
        }

        // create base query
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<UserDto>()
            .From<UserDto>();

        // apply query
        if (query != null)
        {
            sql = new SqlTranslator<IUser>(sql, query).Translate();
        }

        // get sorted and filtered sql
        Sql<ISqlContext> sqlNodeIdsWithSort =
            ApplySort(ApplyFilter(sql, filterSql, query != null), orderBy, orderDirection);

        // get a page of results and total count
        Page<UserDto>? pagedResult = Database.Page<UserDto>(pageIndex + 1, pageSize, sqlNodeIdsWithSort);
        totalRecords = Convert.ToInt32(pagedResult.TotalItems);

        // map references
        PerformGetReferencedDtos(pagedResult.Items);
        return pagedResult.Items.Select(x => UserFactory.BuildEntity(_globalSettings, x));
    }

    private Sql<ISqlContext> ApplyFilter(Sql<ISqlContext> sql, Sql<ISqlContext>? filterSql, bool hasWhereClause)
    {
        if (filterSql == null)
        {
            return sql;
        }

        //ensure we don't append a WHERE if there is already one
        var args = filterSql.Arguments;
        var sqlFilter = hasWhereClause
            ? filterSql.SQL
            : " WHERE " + filterSql.SQL.TrimStart("AND ");

        sql.Append(SqlContext.Sql(sqlFilter, args));

        return sql;
    }

    private Sql<ISqlContext> ApplySort(Sql<ISqlContext> sql, Expression<Func<IUser, object?>>? orderBy,
        Direction orderDirection)
    {
        if (orderBy == null)
        {
            return sql;
        }

        MemberInfo? expressionMember = ExpressionHelper.GetMemberInfo(orderBy);
        BaseMapper mapper = _mapperCollection[typeof(IUser)];
        var mappedField = mapper.Map(expressionMember?.Name);

        if (mappedField.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Could not find a mapping for the column specified in the orderBy clause");
        }

        // beware! NPoco paging code parses the query to isolate the ORDER BY fragment,
        // using a regex that wants "([\w\.\[\]\(\)\s""`,]+)" - meaning that anything
        // else in orderBy is going to break NPoco / not be detected

        // beware! NPoco paging code (in PagingHelper) collapses everything [foo].[bar]
        // to [bar] only, so we MUST use aliases, cannot use [table].[field]

        // beware! pre-2012 SqlServer is using a convoluted syntax for paging, which
        // includes "SELECT ROW_NUMBER() OVER (ORDER BY ...) poco_rn FROM SELECT (...",
        // so anything added here MUST also be part of the inner SELECT statement, ie
        // the original statement, AND must be using the proper alias, as the inner SELECT
        // will hide the original table.field names entirely

        var orderByField = sql.GetAliasedField(mappedField);

        if (orderDirection == Direction.Ascending)
        {
            sql.OrderBy(orderByField);
        }
        else
        {
            sql.OrderByDescending(orderByField);
        }

        return sql;
    }

    public IEnumerable<IUser> GetNextUsers(int id, int count)
    {
        Sql<ISqlContext> idsQuery = SqlContext.Sql()
            .Select<UserDto>(x => x.Id)
            .From<UserDto>()
            .Where<UserDto>(x => x.Id >= id)
            .OrderBy<UserDto>(x => x.Id);

        // first page is index 1, not zero
        var ids = Database.Page<int>(1, count, idsQuery).Items.ToArray();

        // now get the actual users and ensure they are ordered properly (same clause)
        return ids.Length == 0
            ? Enumerable.Empty<IUser>()
            : GetMany(ids).OrderBy(x => x.Id) ?? Enumerable.Empty<IUser>();
    }

    #endregion
}
