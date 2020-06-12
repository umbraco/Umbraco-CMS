﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/>, <see cref="IMembershipUser"/> and eventually Backoffice Users.
    /// </summary>
    public class UserService : ScopeRepositoryService, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IGlobalSettings _globalSettings;
        private readonly bool _isUpgrading;

        public UserService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IRuntimeState runtimeState,
            IUserRepository userRepository, IUserGroupRepository userGroupRepository, IGlobalSettings globalSettings)
            : base(provider, logger, eventMessagesFactory)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _globalSettings = globalSettings;
            _isUpgrading = runtimeState.Level == RuntimeLevel.Install || runtimeState.Level == RuntimeLevel.Upgrade;
        }

        #region Implementation of IMembershipUserService

        /// <summary>
        /// Checks if a User with the username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns><c>True</c> if the User exists otherwise <c>False</c></returns>
        public bool Exists(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.Exists(username);
            }
        }

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <remarks>The user will be saved in the database and returned with an Id</remarks>
        /// <param name="username">Username of the user to create</param>
        /// <param name="email">Email of the user to create</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser CreateUserWithIdentity(string username, string email)
        {
            return CreateUserWithIdentity(username, email, string.Empty);
        }

        /// <summary>
        /// Creates and persists a new <see cref="IUser"/>
        /// </summary>
        /// <param name="username">Username of the <see cref="IUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Not used for users</param>
        /// <returns><see cref="IUser"/></returns>
        IUser IMembershipMemberService<IUser>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias)
        {
            return CreateUserWithIdentity(username, email, passwordValue);
        }

        /// <summary>
        /// Creates and persists a new <see cref="IUser"/>
        /// </summary>
        /// <param name="username">Username of the <see cref="IUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Alias of the Type</param>
        /// <param name="isApproved">Is the member approved</param>
        /// <returns><see cref="IUser"/></returns>
        IUser IMembershipMemberService<IUser>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias, bool isApproved)
        {
            return CreateUserWithIdentity(username, email, passwordValue, isApproved);
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="isApproved">Is the user approved</param>
        /// <returns><see cref="IUser"/></returns>
        private IUser CreateUserWithIdentity(string username, string email, string passwordValue, bool isApproved = true)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(username));

            // TODO: PUT lock here!!

            User user;
            using (var scope = ScopeProvider.CreateScope())
            {
                var loginExists = scope.Database.ExecuteScalar<int>("SELECT COUNT(id) FROM umbracoUser WHERE userLogin = @Login", new { Login = username }) != 0;
                if (loginExists)
                    throw new ArgumentException("Login already exists"); // causes rollback // causes rollback

                user = new User
                {
                    DefaultToLiveEditing = false,
                    Email = email,
                    Language = _globalSettings.DefaultUILanguage,
                    Name = username,
                    RawPasswordValue = passwordValue,
                    Username = username,
                    IsLockedOut = false,
                    IsApproved = isApproved
                };

                var saveEventArgs = new SaveEventArgs<IUser>(user);
                if (scope.Events.DispatchCancelable(SavingUser, this, saveEventArgs))
                {
                    scope.Complete();
                    return user;
                }

                _userRepository.Save(user);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedUser, this, saveEventArgs);
                scope.Complete();
            }

            return user;
        }

        /// <summary>
        /// Gets a User by its integer id
        /// </summary>
        /// <param name="id"><see cref="System.int"/> Id</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IUser"/> by its provider key
        /// </summary>
        /// <param name="id">Id to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByProviderKey(object id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt.Success ? GetById(asInt.Result) : null;
        }

        /// <summary>
        /// Get an <see cref="IUser"/> by email
        /// </summary>
        /// <param name="email">Email to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByEmail(string email)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IUser>().Where(x => x.Email.Equals(email));
                return _userRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get an <see cref="IUser"/> by username
        /// </summary>
        /// <param name="username">Username to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByUsername(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                try
                {
                    return _userRepository.GetByUsername(username, includeSecurityData: true);
                }
                catch (DbException)
                {
                    // TODO: refactor users/upgrade
                    // currently kinda accepting anything on upgrade, but that won't deal with all cases
                    // so we need to do it differently, see the custom UmbracoPocoDataBuilder which should
                    // be better BUT requires that the app restarts after the upgrade!
                    if (_isUpgrading)
                    {
                        //NOTE: this will not be cached
                        return _userRepository.GetByUsername(username, includeSecurityData: false);
                    }
                    
                    throw;
                }
            }
        }

        /// <summary>
        /// Disables an <see cref="IUser"/>
        /// </summary>
        /// <param name="membershipUser"><see cref="IUser"/> to disable</param>
        public void Delete(IUser membershipUser)
        {
            //disable
            membershipUser.IsApproved = false;

            Save(membershipUser);
        }

        /// <summary>
        /// Deletes or disables a User
        /// </summary>
        /// <param name="user"><see cref="IUser"/> to delete</param>
        /// <param name="deletePermanently"><c>True</c> to permanently delete the user, <c>False</c> to disable the user</param>
        public void Delete(IUser user, bool deletePermanently)
        {
            if (deletePermanently == false)
            {
                Delete(user);
            }
            else
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    var deleteEventArgs = new DeleteEventArgs<IUser>(user);
                    if (scope.Events.DispatchCancelable(DeletingUser, this, deleteEventArgs))
                    {
                        scope.Complete();
                        return;
                    }

                    _userRepository.Delete(user);

                    deleteEventArgs.CanCancel = false;
                    scope.Events.Dispatch(DeletedUser, this, deleteEventArgs);
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Saves an <see cref="IUser"/>
        /// </summary>
        /// <param name="entity"><see cref="IUser"/> to Save</param>
        /// <param name="raiseEvents">Optional parameter to raise events.
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IUser entity, bool raiseEvents = true)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IUser>(entity);
                if (raiseEvents && scope.Events.DispatchCancelable(SavingUser, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(entity.Username))
                    throw new ArgumentException("Empty username.", nameof(entity));

                if (string.IsNullOrWhiteSpace(entity.Name))
                    throw new ArgumentException("Empty name.", nameof(entity));

                //Now we have to check for backwards compat hacks, we'll need to process any groups
                //to save first before we update the user since these groups might be new groups.

                var explicitUser = entity as User;
                if (explicitUser != null && explicitUser.GroupsToSave.Count > 0)
                {
                    foreach (var userGroup in explicitUser.GroupsToSave)
                    {
                        _userGroupRepository.Save(userGroup);
                    }
                }

                try
                {
                    _userRepository.Save(entity);
                    if (raiseEvents)
                    {
                        saveEventArgs.CanCancel = false;
                        scope.Events.Dispatch(SavedUser, this, saveEventArgs);
                    }

                    scope.Complete();
                }
                catch (DbException ex)
                {
                    // if we are upgrading and an exception occurs, log and swallow it
                    if (_isUpgrading == false) throw;

                    Logger.Warn<UserService>(ex, "An error occurred attempting to save a user instance during upgrade, normally this warning can be ignored");

                    // we don't want the uow to rollback its scope!
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Saves a list of <see cref="IUser"/> objects
        /// </summary>
        /// <param name="entities"><see cref="IEnumerable{IUser}"/> to save</param>
        /// <param name="raiseEvents">Optional parameter to raise events.
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IEnumerable<IUser> entities, bool raiseEvents = true)
        {
            var entitiesA = entities.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IUser>(entitiesA);
                if (raiseEvents && scope.Events.DispatchCancelable(SavingUser, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                foreach (var user in entitiesA)
                {
                    if (string.IsNullOrWhiteSpace(user.Username))
                        throw new ArgumentException("Empty username.", nameof(entities));

                    if (string.IsNullOrWhiteSpace(user.Name))
                        throw new ArgumentException("Empty name.", nameof(entities));

                    _userRepository.Save(user);

                    //Now we have to check for backwards compat hacks
                    //Now we have to check for backwards compat hacks
                    var explicitUser = user as User;
                    if (explicitUser != null && explicitUser.GroupsToSave.Count > 0)
                    {
                        foreach (var userGroup in explicitUser.GroupsToSave)
                        {
                            _userGroupRepository.Save(userGroup);
                        }
                    }
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(SavedUser, this, saveEventArgs);
                }

                //commit the whole lot in one go
                //commit the whole lot in one go
                scope.Complete();
            }
        }

        /// <summary>
        /// This is just the default user group that the membership provider will use
        /// </summary>
        /// <returns></returns>
        public string GetDefaultMemberType()
        {
            return Constants.Security.WriterGroupAlias;
        }

        /// <summary>
        /// Finds a list of <see cref="IUser"/> objects by a partial email string
        /// </summary>
        /// <param name="emailStringToMatch">Partial email string to match</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.StartsWith"/></param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> FindByEmail(string emailStringToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IUser>();

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query.Where(member => member.Email.Equals(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.Contains:
                        query.Where(member => member.Email.Contains(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query.Where(member => member.Email.StartsWith(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query.Where(member => member.Email.EndsWith(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        query.Where(member => member.Email.SqlWildcard(emailStringToMatch, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                return _userRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Email);
            }
        }

        /// <summary>
        /// Finds a list of <see cref="IUser"/> objects by a partial username
        /// </summary>
        /// <param name="login">Partial username to match</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.StartsWith"/></param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> FindByUsername(string login, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IUser>();

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query.Where(member => member.Username.Equals(login));
                        break;
                    case StringPropertyMatchType.Contains:
                        query.Where(member => member.Username.Contains(login));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query.Where(member => member.Username.StartsWith(login));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query.Where(member => member.Username.EndsWith(login));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        query.Where(member => member.Email.SqlWildcard(login, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                return _userRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Username);
            }
        }

        /// <summary>
        /// Gets the total number of Users based on the count type
        /// </summary>
        /// <remarks>
        /// The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any members
        /// that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact science
        /// but that is how MS have made theirs so we'll follow that principal.
        /// </remarks>
        /// <param name="countType"><see cref="MemberCountType"/> to count by</param>
        /// <returns><see cref="System.int"/> with number of Users for passed in type</returns>
        public int GetCount(MemberCountType countType)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                IQuery<IUser> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = Query<IUser>();
                        break;
                    case MemberCountType.Online:
                        throw new NotImplementedException();
                    //var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                    //query =
                    //    Query<IMember>.Builder.Where(
                    //        x =>
                    //        ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.LastLoginDate &&
                    //        ((Member)x).DateTimePropertyValue > fromDate);
                    //return repository.GetCountByQuery(query);
                    //var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                    //query =
                    //    Query<IMember>.Builder.Where(
                    //        x =>
                    //        ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.LastLoginDate &&
                    //        ((Member)x).DateTimePropertyValue > fromDate);
                    //return repository.GetCountByQuery(query);
                    case MemberCountType.LockedOut:
                        query = Query<IUser>().Where(x => x.IsLockedOut);
                        break;
                    case MemberCountType.Approved:
                        query = Query<IUser>().Where(x => x.IsApproved);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(countType));
                }

                return _userRepository.GetCountByQuery(query);
            }
        }

        public Guid CreateLoginSession(int userId, string requestingIpAddress)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var session = _userRepository.CreateLoginSession(userId, requestingIpAddress);
                scope.Complete();
                return session;
            }
        }

        public int ClearLoginSessions(int userId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var count = _userRepository.ClearLoginSessions(userId);
                scope.Complete();
                return count;
            }
        }

        public void ClearLoginSession(Guid sessionId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _userRepository.ClearLoginSession(sessionId);
                scope.Complete();
            }
        }

        public bool ValidateLoginSession(int userId, Guid sessionId)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.ValidateLoginSession(userId, sessionId);
            }
        }
        public IDictionary<UserState, int> GetUserStates()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.GetUserStates();
            }
        }

        public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection, UserState[] userState = null, string[] userGroups = null, string filter = null)
        {
            IQuery<IUser> filterQuery = null;
            if (filter.IsNullOrWhiteSpace() == false)
            {
                filterQuery = Query<IUser>().Where(x => x.Name.Contains(filter) || x.Username.Contains(filter));
            }

            return GetAll(pageIndex, pageSize, out totalRecords, orderBy, orderDirection, userState, userGroups, null, filterQuery);
        }

        public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection, UserState[] userState = null, string[] includeUserGroups = null, string[] excludeUserGroups = null, IQuery<IUser> filter = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                Expression<Func<IUser, object>> sort;
                switch (orderBy.ToUpperInvariant())
                {
                    case "USERNAME":
                        sort = member => member.Username;
                        break;
                    case "LANGUAGE":
                        sort = member => member.Language;
                        break;
                    case "NAME":
                        sort = member => member.Name;
                        break;
                    case "EMAIL":
                        sort = member => member.Email;
                        break;
                    case "ID":
                        sort = member => member.Id;
                        break;
                    case "CREATEDATE":
                        sort = member => member.CreateDate;
                        break;
                    case "UPDATEDATE":
                        sort = member => member.UpdateDate;
                        break;
                    case "ISAPPROVED":
                        sort = member => member.IsApproved;
                        break;
                    case "ISLOCKEDOUT":
                        sort = member => member.IsLockedOut;
                        break;
                    case "LASTLOGINDATE":
                        sort = member => member.LastLoginDate;
                        break;
                    default:
                        throw new IndexOutOfRangeException("The orderBy parameter " + orderBy + " is not valid");
                }

                return _userRepository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, sort, orderDirection, includeUserGroups, excludeUserGroups, userState, filter);
            }
        }

        /// <summary>
        /// Gets a list of paged <see cref="IUser"/> objects
        /// </summary>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Name);
            }
        }

        internal IEnumerable<IUser> GetNextUsers(int id, int count)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return ((UserRepository) _userRepository).GetNextUsers(id, count);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> GetAllInGroup(int groupId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.GetAllInGroup(groupId);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects not associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> GetAllNotInGroup(int groupId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                return _userRepository.GetAllNotInGroup(groupId);
            }
        }

        #endregion

        #region Implementation of IUserService

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileById(int id)
        {
            //This is called a TON. Go get the full user from cache which should already be IProfile
            var fullUser = GetUserById(id);
            if (fullUser == null) return null;
            var asProfile = fullUser as IProfile;
            return asProfile ?? new UserProfile(fullUser.Id, fullUser.Name);
        }

        /// <summary>
        /// Gets a profile by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileByUserName(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.GetProfile(username);
            }
        }

        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id">Id of the user to retrieve</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetUserById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                try
                {
                    return _userRepository.Get(id);
                }
                catch (DbException)
                {
                    // TODO: refactor users/upgrade
                    // currently kinda accepting anything on upgrade, but that won't deal with all cases
                    // so we need to do it differently, see the custom UmbracoPocoDataBuilder which should
                    // be better BUT requires that the app restarts after the upgrade!
                    if (_isUpgrading)
                    {
                        //NOTE: this will not be cached
                        return _userRepository.Get(id, includeSecurityData: false);
                    }
                    
                    throw;
                }
            }
        }

        public IEnumerable<IUser> GetUsersById(params int[] ids)
        {
            if (ids.Length <= 0) return Enumerable.Empty<IUser>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Replaces the same permission set for a single group to any number of entities
        /// </summary>
        /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified group.</remarks>
        /// <param name="groupId">Id of the group</param>
        /// <param name="permissions">Permissions as enumerable list of <see cref="char"/> If nothing is specified all permissions are removed.</param>
        /// <param name="entityIds">Specify the nodes to replace permissions for. </param>
        public void ReplaceUserGroupPermissions(int groupId, IEnumerable<char> permissions, params int[] entityIds)
        {
            if (entityIds.Length == 0)
                return;

            using (var scope = ScopeProvider.CreateScope())
            {
                _userGroupRepository.ReplaceGroupPermissions(groupId, permissions, entityIds);
                scope.Complete();

                var assigned = permissions.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToArray();
                scope.Events.Dispatch(UserGroupPermissionsAssigned, this,
                    new SaveEventArgs<EntityPermission>(entityIds.Select(x => new EntityPermission(groupId, x, assigned)).ToArray(), false));
            }
        }

        /// <summary>
        /// Assigns the same permission set for a single user group to any number of entities
        /// </summary>
        /// <param name="groupId">Id of the user group</param>
        /// <param name="permission"></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for</param>
        public void AssignUserGroupPermission(int groupId, char permission, params int[] entityIds)
        {
            if (entityIds.Length == 0)
                return;

            using (var scope = ScopeProvider.CreateScope())
            {
                _userGroupRepository.AssignGroupPermission(groupId, permission, entityIds);
                scope.Complete();

                var assigned = new[] { permission.ToString(CultureInfo.InvariantCulture) };
                scope.Events.Dispatch(UserGroupPermissionsAssigned, this,
                    new SaveEventArgs<EntityPermission>(entityIds.Select(x => new EntityPermission(groupId, x, assigned)).ToArray(), false));
            }
        }

        /// <summary>
        /// Gets all UserGroups or those specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserGroups to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserGroup"/></returns>
        public IEnumerable<IUserGroup> GetAllUserGroups(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userGroupRepository.GetMany(ids).OrderBy(x => x.Name);
            }
        }

        public IEnumerable<IUserGroup> GetUserGroupsByAlias(params string[] aliases)
        {
            if (aliases.Length == 0) return Enumerable.Empty<IUserGroup>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IUserGroup>().Where(x => aliases.SqlIn(x.Alias));
                var contents = _userGroupRepository.Get(query);
                return contents.WhereNotNull().ToArray();
            }
        }

        /// <summary>
        /// Gets a UserGroup by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserGroup to retrieve</param>
        /// <returns><see cref="IUserGroup"/></returns>
        public IUserGroup GetUserGroupByAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentException("Value cannot be null or whitespace.", "alias");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IUserGroup>().Where(x => x.Alias == alias);
                var contents = _userGroupRepository.Get(query);
                return contents.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a UserGroup by its Id
        /// </summary>
        /// <param name="id">Id of the UserGroup to retrieve</param>
        /// <returns><see cref="IUserGroup"/></returns>
        public IUserGroup GetUserGroupById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userGroupRepository.Get(id);
            }
        }

        /// <summary>
        /// Saves a UserGroup
        /// </summary>
        /// <param name="userGroup">UserGroup to save</param>
        /// <param name="userIds">
        /// If null than no changes are made to the users who are assigned to this group, however if a value is passed in
        /// than all users will be removed from this group and only these users will be added
        /// </param>
        /// <param name="raiseEvents">Optional parameter to raise events.
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IUserGroup userGroup, int[] userIds = null, bool raiseEvents = true)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // we need to figure out which users have been added / removed, for audit purposes
                var empty = new IUser[0];
                var addedUsers = empty;
                var removedUsers = empty;

                if (userIds != null)
                {
                    var groupUsers = userGroup.HasIdentity ? _userRepository.GetAllInGroup(userGroup.Id).ToArray() : empty;
                    var xGroupUsers = groupUsers.ToDictionary(x => x.Id, x => x);
                    var groupIds = groupUsers.Select(x => x.Id).ToArray();

                    addedUsers = _userRepository.GetMany(userIds.Except(groupIds).ToArray()).Where(x => x.Id != 0).ToArray();
                    removedUsers = groupIds.Except(userIds).Select(x => xGroupUsers[x]).Where(x => x.Id != 0).ToArray();
                }

                var saveEventArgs = new SaveEventArgs<UserGroupWithUsers>(new UserGroupWithUsers(userGroup, addedUsers, removedUsers));

                if (raiseEvents && scope.Events.DispatchCancelable(SavingUserGroup, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _userGroupRepository.AddOrUpdateGroupWithUsers(userGroup, userIds);

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(SavedUserGroup, this, saveEventArgs);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes a UserGroup
        /// </summary>
        /// <param name="userGroup">UserGroup to delete</param>
        public void DeleteUserGroup(IUserGroup userGroup)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IUserGroup>(userGroup);
                if (scope.Events.DispatchCancelable(DeletingUserGroup, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _userGroupRepository.Delete(userGroup);

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedUserGroup, this, deleteEventArgs);

                scope.Complete();
            }
        }

        /// <summary>
        /// Removes a specific section from all users
        /// </summary>
        /// <remarks>This is useful when an entire section is removed from config</remarks>
        /// <param name="sectionAlias">Alias of the section to remove</param>
        public void DeleteSectionFromAllUserGroups(string sectionAlias)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var assignedGroups = _userGroupRepository.GetGroupsAssignedToSection(sectionAlias);
                foreach (var group in assignedGroups)
                {
                    //now remove the section for each user and commit
                    //now remove the section for each user and commit
                    group.RemoveAllowedSection(sectionAlias);
                    _userGroupRepository.Save(group);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Get explicitly assigned permissions for a user and optional node ids
        /// </summary>
        /// <param name="user">User to retrieve permissions for</param>
        /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public EntityPermissionCollection GetPermissions(IUser user, params int[] nodeIds)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userGroupRepository.GetPermissions(user.Groups.ToArray(), true, nodeIds);
            }
        }

        /// <summary>
        /// Get explicitly assigned permissions for a group and optional node Ids
        /// </summary>
        /// <param name="groups">Groups to retrieve permissions for</param>
        /// <param name="fallbackToDefaultPermissions">
        /// Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        private IEnumerable<EntityPermission> GetPermissions(IReadOnlyUserGroup[] groups, bool fallbackToDefaultPermissions, params int[] nodeIds)
        {
            if (groups == null) throw new ArgumentNullException(nameof(groups));

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userGroupRepository.GetPermissions(groups, fallbackToDefaultPermissions, nodeIds);
            }
        }

        /// <summary>
        /// Get explicitly assigned permissions for a group and optional node Ids
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="fallbackToDefaultPermissions">
        ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public EntityPermissionCollection GetPermissions(IUserGroup[] groups, bool fallbackToDefaultPermissions, params int[] nodeIds)
        {
            if (groups == null) throw new ArgumentNullException(nameof(groups));
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _userGroupRepository.GetPermissions(groups.Select(x => x.ToReadOnlyGroup()).ToArray(), fallbackToDefaultPermissions, nodeIds);
            }
        }
        /// <summary>
        /// Gets the implicit/inherited permissions for the user for the given path
        /// </summary>
        /// <param name="user">User to check permissions for</param>
        /// <param name="path">Path to check permissions for</param>
        public EntityPermissionSet GetPermissionsForPath(IUser user, string path)
        {
            var nodeIds = path.GetIdsFromPathReversed();

            if (nodeIds.Length == 0)
                return EntityPermissionSet.Empty();

            //collect all permissions structures for all nodes for all groups belonging to the user
            var groupPermissions = GetPermissionsForPath(user.Groups.ToArray(), nodeIds, fallbackToDefaultPermissions: true).ToArray();

            return CalculatePermissionsForPathForUser(groupPermissions, nodeIds);
        }

        /// <summary>
        /// Gets the permissions for the provided group and path
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="path">Path to check permissions for</param>
        /// <param name="fallbackToDefaultPermissions">
        ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        /// <returns>String indicating permissions for provided user and path</returns>
        public EntityPermissionSet GetPermissionsForPath(IUserGroup[] groups, string path, bool fallbackToDefaultPermissions = false)
        {
            var nodeIds = path.GetIdsFromPathReversed();

            if (nodeIds.Length == 0)
                return EntityPermissionSet.Empty();

            //collect all permissions structures for all nodes for all groups
            var groupPermissions = GetPermissionsForPath(groups.Select(x => x.ToReadOnlyGroup()).ToArray(), nodeIds, fallbackToDefaultPermissions: true).ToArray();

            return CalculatePermissionsForPathForUser(groupPermissions, nodeIds);
        }

        private EntityPermissionCollection GetPermissionsForPath(IReadOnlyUserGroup[] groups, int[] pathIds, bool fallbackToDefaultPermissions = false)
        {
            if (pathIds.Length == 0)
                return new EntityPermissionCollection(Enumerable.Empty<EntityPermission>());

            //get permissions for all nodes in the path by group
            var permissions = GetPermissions(groups, fallbackToDefaultPermissions, pathIds)
                .GroupBy(x => x.UserGroupId);

            return new EntityPermissionCollection(
                permissions.Select(x => GetPermissionsForPathForGroup(x, pathIds, fallbackToDefaultPermissions)));
        }

        /// <summary>
        /// This performs the calculations for inherited nodes based on this http://issues.umbraco.org/issue/U4-10075#comment=67-40085
        /// </summary>
        /// <param name="groupPermissions"></param>
        /// <param name="pathIds"></param>
        /// <returns></returns>
        internal static EntityPermissionSet CalculatePermissionsForPathForUser(
            EntityPermission[] groupPermissions,
            int[] pathIds)
        {
            // not sure this will ever happen, it shouldn't since this should return defaults, but maybe those are empty?
            if (groupPermissions.Length == 0 || pathIds.Length == 0)
                return EntityPermissionSet.Empty();

            //The actual entity id being looked at (deepest part of the path)
            var entityId = pathIds[0];

            var resultPermissions = new EntityPermissionCollection();

            //create a grouped by dictionary of another grouped by dictionary
            var permissionsByGroup = groupPermissions
                .GroupBy(x => x.UserGroupId)
                .ToDictionary(
                    x => x.Key,
                    x => x.GroupBy(a => a.EntityId).ToDictionary(a => a.Key, a => a.ToArray()));

            //iterate through each group
            foreach (var byGroup in permissionsByGroup)
            {
                var added = false;

                //iterate deepest to shallowest
                foreach (var pathId in pathIds)
                {
                    EntityPermission[] permissionsForNodeAndGroup;
                    if (byGroup.Value.TryGetValue(pathId, out permissionsForNodeAndGroup) == false)
                        continue;

                    //In theory there will only be one EntityPermission in this group
                    // but there's nothing stopping the logic of this method
                    // from having more so we deal with it here
                    foreach (var entityPermission in permissionsForNodeAndGroup)
                    {
                        if (entityPermission.IsDefaultPermissions == false)
                        {
                            //explicit permission found so we'll append it and move on, the collection is a hashset anyways
                            //so only supports adding one element per groupid/contentid
                            resultPermissions.Add(entityPermission);
                            added = true;
                            break;
                        }
                    }

                    //if the permission has been added for this group and this branch then we can exit this loop
                    if (added)
                        break;
                }

                if (added == false && byGroup.Value.Count > 0)
                {
                    //if there was no explicit permissions assigned in this branch for this group, then we will
                    //add the group's default permissions
                    resultPermissions.Add(byGroup.Value[entityId][0]);
                }

            }

            var permissionSet = new EntityPermissionSet(entityId, resultPermissions);
            return permissionSet;
        }

        /// <summary>
        /// Returns the resulting permission set for a group for the path based on all permissions provided for the branch
        /// </summary>
        /// <param name="pathPermissions">
        /// The collective set of permissions provided to calculate the resulting permissions set for the path
        /// based on a single group
        /// </param>
        /// <param name="pathIds">Must be ordered deepest to shallowest (right to left)</param>
        /// <param name="fallbackToDefaultPermissions">
        /// Flag indicating if we want to include the default group permissions for each result if there are not explicit permissions set
        /// </param>
        /// <returns></returns>
        internal static EntityPermission GetPermissionsForPathForGroup(
            IEnumerable<EntityPermission> pathPermissions,
            int[] pathIds,
            bool fallbackToDefaultPermissions = false)
        {
            //get permissions for all nodes in the path
            var permissionsByEntityId = pathPermissions.ToDictionary(x => x.EntityId, x => x);

            //then the permissions assigned to the path will be the 'deepest' node found that has permissions
            foreach (var id in pathIds)
            {
                EntityPermission permission;
                if (permissionsByEntityId.TryGetValue(id, out permission))
                {
                    //don't return the default permissions if that is the one assigned here (we'll do that below if nothing was found)
                    if (permission.IsDefaultPermissions == false)
                        return permission;
                }
            }

            //if we've made it here it means that no implicit/inherited permissions were found so we return the defaults if that is specified
            if (fallbackToDefaultPermissions == false)
                return null;

            return permissionsByEntityId[pathIds[0]];
        }

        /// <summary>
        /// Checks in a set of permissions associated with a user for those related to a given nodeId
        /// </summary>
        /// <param name="permissions">The set of permissions</param>
        /// <param name="nodeId">The node Id</param>
        /// <param name="assignedPermissions">The permissions to return</param>
        /// <returns>True if permissions for the given path are found</returns>
        public static bool TryGetAssignedPermissionsForNode(IList<EntityPermission> permissions,
            int nodeId,
            out string assignedPermissions)
        {
            if (permissions.Any(x => x.EntityId == nodeId))
            {
                var found = permissions.First(x => x.EntityId == nodeId);
                var assignedPermissionsArray = found.AssignedPermissions.ToList();

                // Working with permissions assigned directly to a user AND to their groups, so maybe several per node
                // and we need to get the most permissive set
                foreach (var permission in permissions.Where(x => x.EntityId == nodeId).Skip(1))
                {
                    AddAdditionalPermissions(assignedPermissionsArray, permission.AssignedPermissions);
                }

                assignedPermissions = string.Join("", assignedPermissionsArray);
                return true;
            }

            assignedPermissions = string.Empty;
            return false;
        }

        private static void AddAdditionalPermissions(List<string> assignedPermissions, string[] additionalPermissions)
        {
            var permissionsToAdd = additionalPermissions
                .Where(x => assignedPermissions.Contains(x) == false);
            assignedPermissions.AddRange(permissionsToAdd);
        }

        #endregion

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUser>> SavingUser;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUser>> SavedUser;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUser>> DeletingUser;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUser>> DeletedUser;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<UserGroupWithUsers>> SavingUserGroup;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<UserGroupWithUsers>> SavedUserGroup;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserGroup>> DeletingUserGroup;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserGroup>> DeletedUserGroup;

        // TODO: still don't know if we need this yet unless we start caching permissions, but that also means we'll need another
        // event on the ContentService since there's a method there to modify node permissions too, or we can proxy events if needed.
        internal static event TypedEventHandler<IUserService, SaveEventArgs<EntityPermission>> UserGroupPermissionsAssigned;
    }
}
