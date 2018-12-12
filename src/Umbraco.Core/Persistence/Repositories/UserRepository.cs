using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Security;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserRepository for doing CRUD operations for <see cref="IUser"/>
    /// </summary>
    internal class UserRepository : PetaPocoRepositoryBase<int, IUser>, IUserRepository
    {
        private readonly IDictionary<string, string> _passwordConfiguration;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="work"></param>
        /// <param name="cacheHelper"></param>
        /// <param name="logger"></param>
        /// <param name="sqlSyntax"></param>
        /// <param name="passwordConfiguration">
        /// A dictionary specifying the configuration for user passwords. If this is null then no password configuration will be persisted or read.
        /// </param>
        public UserRepository(IScopeUnitOfWork work, CacheHelper cacheHelper, ILogger logger, ISqlSyntaxProvider sqlSyntax,
            IDictionary<string, string> passwordConfiguration = null)
            : base(work, cacheHelper, logger, sqlSyntax)
        {
            _passwordConfiguration = passwordConfiguration;
        }

        #region Overrides of RepositoryBase<int,IUser>

        protected override IUser PerformGet(int id)
        {
            var sql = GetQueryWithGroups();
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql //must be included for relator to work
                .OrderBy<UserDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserGroupDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserStartNodeDto>(d => d.Id, SqlSyntax);

            var dto = Database.Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(new UserGroupRelator().Map, sql)
                .FirstOrDefault();

            if (dto == null)
                return null;

            var user = UserFactory.BuildEntity(dto);
            return user;
        }

        /// <summary>
        /// Returns a user by username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="includeSecurityData">
        /// Can be used for slightly faster user lookups if the result doesn't require security data (i.e. groups, apps & start nodes).
        /// This is really only used for a shim in order to upgrade to 7.6.
        /// </param>
        /// <returns>
        /// A non cached <see cref="IUser"/> instance
        /// </returns>
        public IUser GetByUsername(string username, bool includeSecurityData)
        {
            UserDto dto;
            if (includeSecurityData)
            {
                var sql = GetQueryWithGroups();
                sql.Where<UserDto>(userDto => userDto.Login == username, SqlSyntax);
                sql //must be included for relator to work
                    .OrderBy<UserDto>(d => d.Id, SqlSyntax)
                    .OrderBy<UserGroupDto>(d => d.Id, SqlSyntax)
                    .OrderBy<UserStartNodeDto>(d => d.Id, SqlSyntax);
                dto = Database
                    .Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(
                        new UserGroupRelator().Map, sql)
                    .FirstOrDefault();
            }
            else
            {
                var sql = GetBaseQuery("umbracoUser.*");
                sql.Where<UserDto>(userDto => userDto.Login == username, SqlSyntax);
                dto = Database.FirstOrDefault<UserDto>(sql);
            }

            if (dto == null)
                return null;

            var user = UserFactory.BuildEntity(dto);
            return user;
        }

        /// <summary>
        /// Returns a user by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeSecurityData">
        /// This is really only used for a shim in order to upgrade to 7.6 but could be used 
        /// for slightly faster user lookups if the result doesn't require security data (i.e. groups, apps & start nodes)
        /// </param>
        /// <returns>
        /// A non cached <see cref="IUser"/> instance
        /// </returns>
        public IUser Get(int id, bool includeSecurityData)
        {
            UserDto dto;
            if (includeSecurityData)
            {
                var sql = GetQueryWithGroups();
                sql.Where(GetBaseWhereClause(), new { Id = id });
                sql //must be included for relator to work
                    .OrderBy<UserDto>(d => d.Id, SqlSyntax)
                    .OrderBy<UserGroupDto>(d => d.Id, SqlSyntax)
                    .OrderBy<UserStartNodeDto>(d => d.Id, SqlSyntax);
                dto = Database
                    .Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(
                        new UserGroupRelator().Map, sql)
                    .FirstOrDefault();
            }
            else
            {
                var sql = GetBaseQuery("umbracoUser.*");
                sql.Where(GetBaseWhereClause(), new { Id = id });
                dto = Database.FirstOrDefault<UserDto>(sql);
            }

            if (dto == null)
                return null;

            var user = UserFactory.BuildEntity(dto);
            return user;
        }

        public IProfile GetProfile(string username)
        {
            var sql = GetBaseQuery(false).Where<UserDto>(userDto => userDto.Login == username, SqlSyntax);

            var dto = Database.Fetch<UserDto>(sql)
                .FirstOrDefault();

            if (dto == null)
                return null;

            return new UserProfile(dto.Id, dto.UserName);
        }

        public IProfile GetProfile(int id)
        {
            var sql = GetBaseQuery(false).Where<UserDto>(userDto => userDto.Id == id, SqlSyntax);

            var dto = Database.Fetch<UserDto>(sql)
                .FirstOrDefault();

            if (dto == null)
                return null;

            return new UserProfile(dto.Id, dto.UserName);
        }

        public IDictionary<UserState, int> GetUserStates()
        {
            var sql = @"SELECT '1CountOfAll' AS colName, COUNT(id) AS num FROM umbracoUser 
UNION
SELECT '2CountOfActive' AS colName, COUNT(id) AS num FROM umbracoUser WHERE userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NOT NULL 
UNION
SELECT '3CountOfDisabled' AS colName, COUNT(id) AS num FROM umbracoUser WHERE userDisabled = 1
UNION
SELECT '4CountOfLockedOut' AS colName, COUNT(id) AS num FROM umbracoUser WHERE userNoConsole = 1
UNION
SELECT '5CountOfInvited' AS colName, COUNT(id) AS num FROM umbracoUser WHERE lastLoginDate IS NULL AND userDisabled = 1 AND invitedDate IS NOT NULL
UNION
SELECT '6CountOfDisabled' AS colName, COUNT(id) AS num FROM umbracoUser WHERE userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NULL 
ORDER BY colName";

            var result = Database.Fetch<dynamic>(sql);

            return new Dictionary<UserState, int>
            {
                {UserState.All, (int)result[0].num},
                {UserState.Active, (int)result[1].num},
                {UserState.Disabled, (int)result[2].num},
                {UserState.LockedOut, (int)result[3].num},
                {UserState.Invited, (int)result[4].num},
                {UserState.Inactive, (int) result[5].num}
            };
        }

        public Guid CreateLoginSession(int userId, string requestingIpAddress, bool cleanStaleSessions = true)
        {
            //TODO: I know this doesn't follow the normal repository conventions which would require us to crete a UserSessionRepository
            //and also business logic models for these objects but that's just so overkill for what we are doing
            //and now that everything is properly in a transaction (Scope) there doesn't seem to be much reason for using that anymore
            var now = DateTime.UtcNow;
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
            var found = Database.FirstOrDefault<UserLoginDto>("WHERE sessionId=@sessionId", new {sessionId = sessionId});
            if (found == null || found.UserId != userId || found.LoggedOutUtc.HasValue)
                return false;

            //now detect if there's been a timeout
            if (DateTime.UtcNow - found.LastValidatedUtc > TimeSpan.FromMinutes(GlobalSettings.TimeOutInMinutes))
            {
                //timeout detected, update the record
                ClearLoginSession(sessionId);
                return false;
            }

            //update the validate date
            found.LastValidatedUtc = DateTime.UtcNow;
            Database.Update(found);
            return true;
        }

        public int ClearLoginSessions(int userId)
        {
            //TODO: I know this doesn't follow the normal repository conventions which would require us to crete a UserSessionRepository
            //and also business logic models for these objects but that's just so overkill for what we are doing
            //and now that everything is properly in a transaction (Scope) there doesn't seem to be much reason for using that anymore
            var count = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoUserLogin WHERE userId=@userId", new { userId = userId });
            Database.Execute("DELETE FROM umbracoUserLogin WHERE userId=@userId", new {userId = userId});
            return count;
        }

        public int ClearLoginSessions(TimeSpan timespan)
        {
            //TODO: I know this doesn't follow the normal repository conventions which would require us to crete a UserSessionRepository
            //and also business logic models for these objects but that's just so overkill for what we are doing
            //and now that everything is properly in a transaction (Scope) there doesn't seem to be much reason for using that anymore

            var fromDate = DateTime.UtcNow - timespan;

            var count = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoUserLogin WHERE lastValidatedUtc=@fromDate", new { fromDate = fromDate });
            Database.Execute("DELETE FROM umbracoUserLogin WHERE lastValidatedUtc=@fromDate", new { fromDate = fromDate });
            return count;
        }

        public void ClearLoginSession(Guid sessionId)
        {
            //TODO: I know this doesn't follow the normal repository conventions which would require us to crete a UserSessionRepository
            //and also business logic models for these objects but that's just so overkill for what we are doing
            //and now that everything is properly in a transaction (Scope) there doesn't seem to be much reason for using that anymore
            Database.Execute("UPDATE umbracoUserLogin SET loggedOutUtc=@now WHERE sessionId=@sessionId",
                new { now = DateTime.UtcNow, sessionId = sessionId });
        }

        protected override IEnumerable<IUser> PerformGetAll(params int[] ids)
        {
            var sql = GetQueryWithGroups();
            if (ids.Any())
            {
                sql.Where("umbracoUser.id in (@ids)", new { ids = ids });
            }
            sql //must be included for relator to work
                .OrderBy<UserDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserGroupDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserStartNodeDto>(d => d.Id, SqlSyntax);

            var users = ConvertFromDtos(Database.Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(new UserGroupRelator().Map, sql))
                .ToArray(); // important so we don't iterate twice, if we don't do this we can end up with null values in cache if we were caching.    

            return users;
        }

        protected override IEnumerable<IUser> PerformGetByQuery(IQuery<IUser> query)
        {
            var sqlClause = GetQueryWithGroups();
            var translator = new SqlTranslator<IUser>(sqlClause, query);
            var sql = translator.Translate();
            sql //must be included for relator to work
                .OrderBy<UserDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserGroupDto>(d => d.Id, SqlSyntax)
                .OrderBy<UserStartNodeDto>(d => d.Id, SqlSyntax);

            var dtos = Database.Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(new UserGroupRelator().Map, sql)
                .DistinctBy(x => x.Id);

            var users = ConvertFromDtos(dtos)
                .ToArray(); // important so we don't iterate twice, if we don't do this we can end up with null values in cache if we were caching.    

            return users;
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IUser>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<UserDto>();
            }
            else
            {
                return GetBaseQuery("*");
            }
            return sql;
        }

        /// <summary>
        /// A query to return a user with it's groups and with it's groups sections
        /// </summary>
        /// <returns></returns>
        private Sql GetQueryWithGroups()
        {
            //base query includes user groups
            var sql = GetBaseQuery("umbracoUser.*, umbracoUserGroup.*, umbracoUserGroup2App.*, umbracoUserStartNode.*");
            AddGroupLeftJoin(sql);
            return sql;
        }

        private void AddGroupLeftJoin(Sql sql)
        {
            sql.LeftJoin<User2UserGroupDto>(SqlSyntax)
                .On<User2UserGroupDto, UserDto>(SqlSyntax, dto => dto.UserId, dto => dto.Id)
                .LeftJoin<UserGroupDto>(SqlSyntax)
                .On<UserGroupDto, User2UserGroupDto>(SqlSyntax, dto => dto.Id, dto => dto.UserGroupId)
                .LeftJoin<UserGroup2AppDto>(SqlSyntax)
                .On<UserGroup2AppDto, UserGroupDto>(SqlSyntax, dto => dto.UserGroupId, dto => dto.Id)
                .LeftJoin<UserStartNodeDto>(SqlSyntax)
                .On<UserStartNodeDto, UserDto>(SqlSyntax, dto => dto.UserId, dto => dto.Id);
        }

        private Sql GetBaseQuery(string columns)
        {
            var sql = new Sql();
            sql.Select(columns)
                .From<UserDto>();
            return sql;
        }


        protected override string GetBaseWhereClause()
        {
            return "umbracoUser.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM cmsTask WHERE userId = @Id",
                "DELETE FROM cmsTask WHERE parentUserId = @Id",
                "DELETE FROM umbracoUser2UserGroup WHERE userId = @Id",
                "DELETE FROM umbracoUser2NodeNotify WHERE userId = @Id",
                "DELETE FROM umbracoUser WHERE id = @Id",
                "DELETE FROM umbracoExternalLogin WHERE id = @Id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IUser entity)
        {
            ((User)entity).AddingEntity();

            //ensure security stamp if non
            if (entity.SecurityStamp.IsNullOrWhiteSpace())
            {
                entity.SecurityStamp = Guid.NewGuid().ToString();
            }

            var userDto = UserFactory.BuildDto(entity);

            //Check if we have a known config, we only want to store config for hashing
            //TODO: This logic will need to be updated when we do http://issues.umbraco.org/issue/U4-10089            
            if (_passwordConfiguration != null && _passwordConfiguration.Count > 0)
            {
                var json = JsonConvert.SerializeObject(_passwordConfiguration);
                userDto.PasswordConfig = json;
            }

            var id = Convert.ToInt32(Database.Insert(userDto));
            entity.Id = id;

            if (entity.IsPropertyDirty("StartContentIds") || entity.IsPropertyDirty("StartMediaIds"))
            {
                if (entity.IsPropertyDirty("StartContentIds"))
                {
                    AddingOrUpdateStartNodes(entity, Enumerable.Empty<UserStartNodeDto>(), UserStartNodeDto.StartNodeTypeValue.Content, entity.StartContentIds);
                }
                if (entity.IsPropertyDirty("StartMediaIds"))
                {
                    AddingOrUpdateStartNodes(entity, Enumerable.Empty<UserStartNodeDto>(), UserStartNodeDto.StartNodeTypeValue.Media, entity.StartMediaIds);
                }
            }

            if (entity.IsPropertyDirty("Groups"))
            {
                //lookup all assigned
                var assigned = entity.Groups == null || entity.Groups.Any() == false
                    ? new List<UserGroupDto>()
                    : Database.Fetch<UserGroupDto>("SELECT * FROM umbracoUserGroup WHERE userGroupAlias IN (@aliases)", new { aliases = entity.Groups.Select(x => x.Alias) });

                foreach (var groupDto in assigned)
                {
                    var dto = new User2UserGroupDto
                    {
                        UserGroupId = groupDto.Id,
                        UserId = entity.Id
                    };
                    Database.Insert(dto);
                }
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IUser entity)
        {
            //Updates Modified date 
            ((User)entity).UpdatingEntity();

            //ensure security stamp if non
            if (entity.SecurityStamp.IsNullOrWhiteSpace())
            {
                entity.SecurityStamp = Guid.NewGuid().ToString();
            }

            var userDto = UserFactory.BuildDto(entity);

            //build list of columns to check for saving - we don't want to save the password if it hasn't changed!
            //List the columns to save, NOTE: would be nice to not have hard coded strings here but no real good way around that
            var colsToSave = new Dictionary<string, string>()
            {
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

            //create list of properties that have changed
            var changedCols = colsToSave
                .Where(col => entity.IsPropertyDirty(col.Value))
                .Select(col => col.Key)
                .ToList();

            // DO NOT update the password if it has not changed or if it is null or empty
            if (entity.IsPropertyDirty("RawPasswordValue") && entity.RawPasswordValue.IsNullOrWhiteSpace() == false)
            {
                changedCols.Add("userPassword");

                //special case - when using ASP.Net identity the user manager will take care of updating the security stamp, however
                // when not using ASP.Net identity (i.e. old membership providers), we'll need to take care of updating this manually
                // so we can just detect if that property is dirty, if it's not we'll set it manually
                if (entity.IsPropertyDirty("SecurityStamp") == false)
                {
                    userDto.SecurityStampToken = entity.SecurityStamp = Guid.NewGuid().ToString();
                    changedCols.Add("securityStampToken");
                }

                //Check if we have a known config, we only want to store config for hashing
                //TODO: This logic will need to be updated when we do http://issues.umbraco.org/issue/U4-10089
                if (_passwordConfiguration != null && _passwordConfiguration.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(_passwordConfiguration);
                    userDto.PasswordConfig = json;

                    changedCols.Add("passwordConfig");
                }
            }

            //only update the changed cols
            if (changedCols.Count > 0)
            {
                Database.Update(userDto, changedCols);
            }

            if (entity.IsPropertyDirty("StartContentIds") || entity.IsPropertyDirty("StartMediaIds"))
            {
                var assignedStartNodes = Database.Fetch<UserStartNodeDto>("SELECT * FROM umbracoUserStartNode WHERE userId = @userId", new { userId = entity.Id });
                if (entity.IsPropertyDirty("StartContentIds"))
                {
                    AddingOrUpdateStartNodes(entity, assignedStartNodes, UserStartNodeDto.StartNodeTypeValue.Content, entity.StartContentIds);
                }
                if (entity.IsPropertyDirty("StartMediaIds"))
                {
                    AddingOrUpdateStartNodes(entity, assignedStartNodes, UserStartNodeDto.StartNodeTypeValue.Media, entity.StartMediaIds);
                }
            }

            if (entity.IsPropertyDirty("Groups"))
            {
                //lookup all assigned
                var assigned = entity.Groups == null || entity.Groups.Any() == false
                    ? new List<UserGroupDto>()
                    : Database.Fetch<UserGroupDto>("SELECT * FROM umbracoUserGroup WHERE userGroupAlias IN (@aliases)", new { aliases = entity.Groups.Select(x => x.Alias) });

                //first delete all 
                //TODO: We could do this a nicer way instead of "Nuke and Pave"
                Database.Delete<User2UserGroupDto>("WHERE UserId = @UserId", new { UserId = entity.Id });

                foreach (var groupDto in assigned)
                {
                    var dto = new User2UserGroupDto
                    {
                        UserGroupId = groupDto.Id,
                        UserId = entity.Id
                    };
                    Database.Insert(dto);
                }
            }

            entity.ResetDirtyProperties();
        }

        private void AddingOrUpdateStartNodes(IEntity entity, IEnumerable<UserStartNodeDto> current, UserStartNodeDto.StartNodeTypeValue startNodeType, int[] entityStartIds)
        {
            var assignedIds = current.Where(x => x.StartNodeType == (int)startNodeType).Select(x => x.StartNode).ToArray();

            //remove the ones not assigned to the entity
            var toDelete = assignedIds.Except(entityStartIds).ToArray();
            if (toDelete.Length > 0)
                Database.Delete<UserStartNodeDto>("WHERE UserId = @UserId AND startNode IN (@startNodes)", new { UserId = entity.Id, startNodes = toDelete });
            //add the ones not currently in the db
            var toAdd = entityStartIds.Except(assignedIds).ToArray();
            foreach (var i in toAdd)
            {
                var dto = new UserStartNodeDto
                {
                    StartNode = i,
                    StartNodeType = (int)startNodeType,
                    UserId = entity.Id
                };
                Database.Insert(dto);
            }
        }

        #endregion

        #region Implementation of IUserRepository

        public int GetCountByQuery(IQuery<IUser> query)
        {
            var sqlClause = GetBaseQuery("umbracoUser.id");
            var translator = new SqlTranslator<IUser>(sqlClause, query);
            var subquery = translator.Translate();
            //get the COUNT base query
            var sql = GetBaseQuery(true)
                .Append(new Sql("WHERE umbracoUser.id IN (" + subquery.SQL + ")", subquery.Arguments));

            return Database.ExecuteScalar<int>(sql);
        }

        public bool Exists(string username)
        {
            var sql = new Sql();

            sql.Select("COUNT(*)")
                .From<UserDto>()
                .Where<UserDto>(x => x.UserName == username);

            return Database.ExecuteScalar<int>(sql) > 0;
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        public IEnumerable<IUser> GetAllInGroup(int groupId)
        {
            return GetAllInOrNotInGroup(groupId, true);
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects not associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        public IEnumerable<IUser> GetAllNotInGroup(int groupId)
        {
            return GetAllInOrNotInGroup(groupId, false);
        }

        private IEnumerable<IUser> GetAllInOrNotInGroup(int groupId, bool include)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<UserDto>();

            var innerSql = new Sql();
            innerSql.Select("umbracoUser.id")
                .From<UserDto>()
                .LeftJoin<User2UserGroupDto>()
                .On<UserDto, User2UserGroupDto>(left => left.Id, right => right.UserId)
                .Where("umbracoUser2UserGroup.userGroupId = " + groupId);

            sql.Where(string.Format("umbracoUser.id {0} ({1})",
                include ? "IN" : "NOT IN",
                innerSql.SQL));
            return ConvertFromDtos(Database.Fetch<UserDto>(sql));
        }

        [Obsolete("Use the overload with long operators instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, int pageIndex, int pageSize, out int totalRecords, Expression<Func<IUser, string>> orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException("orderBy");

            // get the referenced column name and find the corresp mapped column name
            var expressionMember = ExpressionHelper.GetMemberInfo(orderBy);
            var mapper = MappingResolver.Current.ResolveMapperByType(typeof(IUser));
            var mappedField = mapper.Map(expressionMember.Name);

            if (mappedField.IsNullOrWhiteSpace())
                throw new ArgumentException("Could not find a mapping for the column specified in the orderBy clause");

            long tr;
            var results = GetPagedResultsByQuery(query, Convert.ToInt64(pageIndex), pageSize, out tr, mappedField, Direction.Ascending);
            totalRecords = Convert.ToInt32(tr);
            return results;
        }

        /// <summary>
        /// Gets paged user results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="includeUserGroups">
        /// A filter to only include user that belong to these user groups
        /// </param>
        /// <param name="excludeUserGroups">
        /// A filter to only include users that do not belong to these user groups
        /// </param>
        /// <param name="userState">Optional parameter to filter by specfied user state</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks>
        /// The query supplied will ONLY work with data specifically on the umbracoUser table because we are using PetaPoco paging (SQL paging)
        /// </remarks>
        public IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, long pageIndex, int pageSize, out long totalRecords,
            Expression<Func<IUser, object>> orderBy, Direction orderDirection,
            string[] includeUserGroups = null,
            string[] excludeUserGroups = null,
            UserState[] userState = null,
            IQuery<IUser> filter = null)
        {
            if (orderBy == null) throw new ArgumentNullException("orderBy");

            // get the referenced column name and find the corresp mapped column name
            var expressionMember = ExpressionHelper.GetMemberInfo(orderBy);
            var mapper = MappingResolver.Current.ResolveMapperByType(typeof(IUser));
            var mappedField = mapper.Map(expressionMember.Name);

            if (mappedField.IsNullOrWhiteSpace())
                throw new ArgumentException("Could not find a mapping for the column specified in the orderBy clause");

            return GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, mappedField, orderDirection, includeUserGroups, excludeUserGroups, userState, filter);
        }



        private IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection,
            string[] includeUserGroups = null,
            string[] excludeUserGroups = null,
            UserState[] userState = null,
            IQuery<IUser> customFilter = null)
        {
            if (string.IsNullOrWhiteSpace(orderBy)) throw new ArgumentException("Value cannot be null or whitespace.", "orderBy");


            Sql filterSql = null;
            var customFilterWheres = customFilter != null ? customFilter.GetWhereClauses().ToArray() : null;
            var hasCustomFilter = customFilterWheres != null && customFilterWheres.Length > 0;
            if (hasCustomFilter
                || (includeUserGroups != null && includeUserGroups.Length > 0) || (excludeUserGroups != null && excludeUserGroups.Length > 0)
                || (userState != null && userState.Length > 0 && userState.Contains(UserState.All) == false))
                filterSql = new Sql();

            if (hasCustomFilter)
            {
                foreach (var filterClause in customFilterWheres)
                {
                    filterSql.Append($"AND ({filterClause.Item1})", filterClause.Item2);
                }
            }

            if (includeUserGroups != null && includeUserGroups.Length > 0)
            {
                const string subQuery = @"AND (umbracoUser.id IN (SELECT DISTINCT umbracoUser.id
		            FROM umbracoUser
		            INNER JOIN umbracoUser2UserGroup ON umbracoUser2UserGroup.userId = umbracoUser.id
		            INNER JOIN umbracoUserGroup ON umbracoUserGroup.id = umbracoUser2UserGroup.userGroupId
		            WHERE umbracoUserGroup.userGroupAlias IN (@userGroups)))";
                filterSql.Append(subQuery, new { userGroups = includeUserGroups });
            }

            if (excludeUserGroups != null && excludeUserGroups.Length > 0)
            {
                const string subQuery = @"AND (umbracoUser.id NOT IN (SELECT DISTINCT umbracoUser.id
		            FROM umbracoUser
		            INNER JOIN umbracoUser2UserGroup ON umbracoUser2UserGroup.userId = umbracoUser.id
		            INNER JOIN umbracoUserGroup ON umbracoUserGroup.id = umbracoUser2UserGroup.userGroupId
		            WHERE umbracoUserGroup.userGroupAlias IN (@userGroups)))";
                filterSql.Append(subQuery, new { userGroups = excludeUserGroups });
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
                        if (appended) sb.Append(" OR ");
                        sb.Append("(userDisabled = 0 AND userNoConsole = 0 AND lastLoginDate IS NULL)");
                        appended = true;
                    }
                    if (userState.Contains(UserState.Disabled))
                    {
                        if (appended) sb.Append(" OR ");
                        sb.Append("(userDisabled = 1)");
                        appended = true;
                    }
                    if (userState.Contains(UserState.LockedOut))
                    {
                        if (appended) sb.Append(" OR ");
                        sb.Append("(userNoConsole = 1)");
                        appended = true;
                    }
                    if (userState.Contains(UserState.Invited))
                    {
                        if (appended) sb.Append(" OR ");
                        sb.Append("(lastLoginDate IS NULL AND userDisabled = 1 AND invitedDate IS NOT NULL)");
                        appended = true;
                    }

                    sb.Append(")");

                    filterSql.Append("AND " + sb);
                }
            }

            // Get base query for returning IDs
            var sqlBaseIds = GetBaseQuery("id");

            if (query == null) query = new Query<IUser>();
            var queryHasWhereClause = query.GetWhereClauses().Any();
            var translatorIds = new SqlTranslator<IUser>(sqlBaseIds, query);
            var sqlQueryIds = translatorIds.Translate();
            var sqlBaseFull = GetBaseQuery("umbracoUser.*, umbracoUserGroup.*, umbracoUserGroup2App.*, umbracoUserStartNode.*");
            var translatorFull = new SqlTranslator<IUser>(sqlBaseFull, query);

            //get sorted and filtered sql
            var sqlNodeIdsWithSort = GetSortedSqlForPagedResults(
                GetFilteredSqlForPagedResults(sqlQueryIds, filterSql, queryHasWhereClause),
                orderDirection, orderBy);

            // Get page of results and total count
            var pagedResult = Database.Page<UserDto>(pageIndex + 1, pageSize, sqlNodeIdsWithSort);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            //NOTE: We need to check the actual items returned, not the 'totalRecords', that is because if you request a page number
            // that doesn't actually have any data on it, the totalRecords will still indicate there are records but there are none in
            // the pageResult.
            if (pagedResult.Items.Any())
            {
                //Create the inner paged query that was used above to get the paged result, we'll use that as the inner sub query
                var args = sqlNodeIdsWithSort.Arguments;
                string sqlStringCount, sqlStringPage;
                Database.BuildPageQueries<UserDto>(pageIndex * pageSize, pageSize, sqlNodeIdsWithSort.SQL, ref args, out sqlStringCount, out sqlStringPage);
                
                var sqlQueryFull = translatorFull.Translate();

                //We need to make this FULL query an inner join on the paged ID query
                var splitQuery = sqlQueryFull.SQL.Split(new[] { "WHERE " }, StringSplitOptions.None);
                var fullQueryWithPagedInnerJoin = new Sql(splitQuery[0])
                    .Append("INNER JOIN (")
                    //join the paged query with the paged query arguments
                    .Append(sqlStringPage, args)
                    .Append(") temp ")
                    .Append("ON umbracoUser.id = temp.id");

                AddGroupLeftJoin(fullQueryWithPagedInnerJoin);

                if (splitQuery.Length > 1)
                {
                    //add the original where clause back with the original arguments
                    fullQueryWithPagedInnerJoin.Where(splitQuery[1], sqlQueryIds.Arguments);
                }   

                //get sorted and filtered sql
                var fullQuery = GetSortedSqlForPagedResults(
                    GetFilteredSqlForPagedResults(fullQueryWithPagedInnerJoin, filterSql, queryHasWhereClause),
                    orderDirection, orderBy);

                var users = ConvertFromDtos(Database.Fetch<UserDto, UserGroupDto, UserGroup2AppDto, UserStartNodeDto, UserDto>(new UserGroupRelator().Map, fullQuery))
                    .ToArray(); // important so we don't iterate twice, if we don't do this we can end up with null values in cache if we were caching.    

                return users;
            }

            return Enumerable.Empty<IUser>();
        }

        private Sql GetFilteredSqlForPagedResults(Sql sql, Sql filterSql, bool hasWhereClause)
        {
            Sql filteredSql;

            // Apply filter
            if (filterSql != null)
            {
                //ensure we don't append a WHERE if there is already one
                var sqlFilter = hasWhereClause
                    ? filterSql.SQL
                    : " WHERE " + filterSql.SQL.TrimStart("AND ");

                //NOTE: this is certainly strange - NPoco handles this much better but we need to re-create the sql
                // instance a couple of times to get the parameter order correct, for some reason the first
                // time the arguments don't show up correctly but the SQL argument parameter names are actually updated
                // accordingly - so we re-create it again. In v8 we don't need to do this and it's already taken care of.

                filteredSql = new Sql(sql.SQL, sql.Arguments);
                var args = filteredSql.Arguments.Concat(filterSql.Arguments).ToArray();
                filteredSql = new Sql(
                    string.Format("{0} {1}", filteredSql.SQL, sqlFilter),
                    args);
                filteredSql = new Sql(filteredSql.SQL, args);
            }
            else
            {
                //copy to var so that the original isn't changed
                filteredSql = new Sql(sql.SQL, sql.Arguments);
            }
            return filteredSql;
        }

        private Sql GetSortedSqlForPagedResults(Sql sql, Direction orderDirection, string orderBy)
        {
            //copy to var so that the original isn't changed
            var sortedSql = new Sql(sql.SQL, sql.Arguments);

            // Apply order according to parameters
            if (string.IsNullOrEmpty(orderBy) == false)
            {
                //each order by param needs to be in a bracket! see: https://github.com/toptensoftware/PetaPoco/issues/177
                var orderByParams = new[] { string.Format("({0})", orderBy) };
                if (orderDirection == Direction.Ascending)
                {
                    sortedSql.OrderBy(orderByParams);
                }
                else
                {
                    sortedSql.OrderByDescending(orderByParams);
                }
            }
            return sortedSql;
        }

        internal IEnumerable<IUser> GetNextUsers(int id, int count)
        {
            var idsQuery = new Sql()
                .Select("umbracoUser.id")
                .From<UserDto>(SqlSyntax)
                .Where<UserDto>(x => x.Id >= id)
                .OrderBy<UserDto>(x => x.Id, SqlSyntax);

            // first page is index 1, not zero
            var ids = Database.Page<int>(1, count, idsQuery).Items.ToArray();

            // now get the actual users and ensure they are ordered properly (same clause)
            return ids.Length == 0 ? Enumerable.Empty<IUser>() : GetAll(ids).OrderBy(x => x.Id);
        }

        #endregion

        private IEnumerable<IUser> ConvertFromDtos(IEnumerable<UserDto> dtos)
        {
            return dtos.Select(UserFactory.BuildEntity);
        }
        
    }
}
