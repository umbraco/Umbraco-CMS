using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Security;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/>, <see cref="IMembershipUser"/> and eventually Backoffice Users.
    /// </summary>
    public class UserService : ScopeRepositoryService, IUserService
    {

        //TODO: We need to change the isUpgrading flag to use an app state enum as described here: http://issues.umbraco.org/issue/U4-6816
        // in the meantime, we will use a boolean which we are currently using during upgrades to ensure that a user object is not persisted during this phase, otherwise
        // exceptions can occur if the db is not in it's correct state.
        internal bool IsUpgrading { get; set; }

        public UserService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            IsUpgrading = false;
        }

        #region Implementation of IMembershipUserService

        /// <summary>
        /// Checks if a User with the username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns><c>True</c> if the User exists otherwise <c>False</c></returns>
        public bool Exists(string username)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.Exists(username);
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
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <returns><see cref="IUser"/></returns>
        private IUser CreateUserWithIdentity(string username, string email, string passwordValue)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Cannot create user with empty username.");
            }

            //TODO: PUT lock here!!

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                var loginExists = uow.Database.ExecuteScalar<int>("SELECT COUNT(id) FROM umbracoUser WHERE userLogin = @Login", new { Login = username }) != 0;
                if (loginExists)
                    throw new ArgumentException("Login already exists");

                var user = new User
                {
                    DefaultToLiveEditing = false,
                    Email = email,
                    Language = GlobalSettings.DefaultUILanguage,
                    Name = username,
                    RawPasswordValue = passwordValue,
                    Username = username,                    
                    IsLockedOut = false,
                    IsApproved = true
                };

                if (uow.Events.DispatchCancelable(SavingUser, this, new SaveEventArgs<IUser>(user)))
                {
                    uow.Commit();
                    return user;
                }

                repository.AddOrUpdate(user);
                uow.Commit();

                uow.Events.Dispatch(SavedUser, this, new SaveEventArgs<IUser>(user, false));

                return user;
            }
        }

        /// <summary>
        /// Gets a User by its integer id
        /// </summary>
        /// <param name="id"><see cref="System.int"/> Id</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.Get(id);
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
            if (asInt.Success)
            {
                return GetById((int)id);
            }

            return null;
        }

        /// <summary>
        /// Get an <see cref="IUser"/> by email
        /// </summary>
        /// <param name="email">Email to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByEmail(string email)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                var query = Query<IUser>.Builder.Where(x => x.Email.Equals(email));
                return repository.GetByQuery(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get an <see cref="IUser"/> by username
        /// </summary>
        /// <param name="username">Username to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByUsername(string username)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                var query = Query<IUser>.Builder.Where(x => x.Username.Equals(username));
                return repository.GetByQuery(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Deletes an <see cref="IUser"/>
        /// </summary>
        /// <param name="membershipUser"><see cref="IUser"/> to Delete</param>
        public void Delete(IUser membershipUser)
        {
            //disable
            membershipUser.IsApproved = false;
            
            Save(membershipUser);
        }
        
        /// <summary>
        /// This is simply a helper method which essentially just wraps the MembershipProvider's ChangePassword method
        /// </summary>
        /// <remarks>
        /// This method exists so that Umbraco developers can use one entry point to create/update users if they choose to.
        /// </remarks>
        /// <param name="user">The user to save the password for</param>
        /// <param name="password">The password to save</param>
        public void SavePassword(IUser user, string password)
        {
            if (user == null) throw new ArgumentNullException("user");

            var provider = MembershipProviderExtensions.GetUsersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider() == false)
                throw new NotSupportedException("When using a non-Umbraco membership provider you must change the user password by using the MembershipProvider.ChangePassword method");

            provider.ChangePassword(user.Username, "", password);

            //go re-fetch the member and update the properties that may have changed
            var result = GetByUsername(user.Username);
            if (result != null)
            {
                //should never be null but it could have been deleted by another thread.
                user.RawPasswordValue = result.RawPasswordValue;
                user.LastPasswordChangeDate = result.LastPasswordChangeDate;
                user.UpdateDate = result.UpdateDate;
            }
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
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    if (uow.Events.DispatchCancelable(DeletingUser, this, new DeleteEventArgs<IUser>(user)))
                    {
                        uow.Commit();
                        return;
                    }
                    var repository = RepositoryFactory.CreateUserRepository(uow);
                    repository.Delete(user);
                    uow.Commit();
                    uow.Events.Dispatch(DeletedUser, this, new DeleteEventArgs<IUser>(user, false));
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
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(SavingUser, this, new SaveEventArgs<IUser>(entity)))
                {
                    uow.Commit();
                    return;
                }

                if (string.IsNullOrWhiteSpace(entity.Username))
                {
                    throw new ArgumentException("Cannot save user with empty username.");
                }

                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    throw new ArgumentException("Cannot save user with empty name.");
                }

                var repository = RepositoryFactory.CreateUserRepository(uow);
                repository.AddOrUpdate(entity);
                try
                {
                    // try to flush the unit of work
                    // ie executes the SQL but does not commit the trx yet
                    // so we are *not* catching commit exceptions
                    uow.Commit();

                    if (raiseEvents)
                        uow.Events.Dispatch(SavedUser, this, new SaveEventArgs<IUser>(entity, false));
                }
                catch (DbException ex)
                {
                    // if we are upgrading and an exception occurs, log and swallow it
                    if (IsUpgrading == false) throw;
                    Logger.WarnWithException<UserService>("An error occurred attempting to save a user instance during upgrade, normally this warning can be ignored", ex);

                    // we don't want the uow to rollback its scope! and yet
                    // we cannot try and uow.Commit() again as it would fail again,
                    // we have to bypass the uow entirely and complete its inner scope.
                    // (when the uow disposes, it won't complete the scope again, just dispose it)
                    uow.Scope.Complete();
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
            var asArray = entities.ToArray();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (raiseEvents)
                {
                    if (uow.Events.DispatchCancelable(SavingUser, this, new SaveEventArgs<IUser>(asArray)))
                    {
                        uow.Commit();
                        return;
                    }
                }
                var repository = RepositoryFactory.CreateUserRepository(uow);
                foreach (var member in asArray)
                {
                    if (string.IsNullOrWhiteSpace(member.Username))
                    {
                        throw new ArgumentException("Cannot save user with empty username.");
                    }
                    if (string.IsNullOrWhiteSpace(member.Name))
                    {
                        throw new ArgumentException("Cannot save user with empty name.");
                    }
                    repository.AddOrUpdate(member);
                }
                //commit the whole lot in one go
                uow.Commit();

                if (raiseEvents)
                    uow.Events.Dispatch(SavedUser, this, new SaveEventArgs<IUser>(asArray, false));
            }


        }

        public string GetDefaultMemberType()
        {
            // User types now being removed, there is no default user type to return
            return "None";
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
        public IEnumerable<IUser> FindByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                var query = new Query<IUser>();

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
                        throw new ArgumentOutOfRangeException("matchType");
                }

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Email);
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
        public IEnumerable<IUser> FindByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                var query = new Query<IUser>();

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
                        throw new ArgumentOutOfRangeException("matchType");
                }

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Username);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);

                IQuery<IUser> query;
                int ret;
                switch (countType)
                {
                    case MemberCountType.All:
                        query = new Query<IUser>();
                        ret = repository.Count(query);
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
                    case MemberCountType.LockedOut:
                        query = Query<IUser>.Builder.Where(x => x.IsLockedOut);
                        ret = repository.GetCountByQuery(query);
                        break;
                    case MemberCountType.Approved:
                        query = Query<IUser>.Builder.Where(x => x.IsApproved);
                        ret = repository.GetCountByQuery(query);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("countType");
                }

                return ret;
            }
        }

        public IDictionary<UserState, int> GetUserStates()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetUserStates();
            }
        }

        public IEnumerable<IUser> GetAll(long pageIndex, int pageSize, out long totalRecords, string orderBy, Direction orderDirection, UserState? userState = null, string[] userGroups = null, string filter = "")
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
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
                    default:
                        throw new IndexOutOfRangeException("The orderBy parameter " + orderBy + " is not valid");
                }

                IQuery<IUser> filterQuery = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IUser>.Builder.Where(x => x.Name.Contains(filter) || x.Username.Contains(filter));
                }

                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, sort, orderDirection, userGroups, userState, filterQuery);
            }
        }

        /// <summary>
        /// Gets a list of paged <see cref="IUser"/> objects
        /// </summary>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IUser> GetAll(int pageIndex, int pageSize, out int totalRecords)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Username);
            }
        }

        internal IEnumerable<IUser> GetNextUsers(int id, int count)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = (UserRepository)RepositoryFactory.CreateUserRepository(uow);
                return repository.GetNextUsers(id, count);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> GetAllInGroup(int groupId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                return repository.GetAllInGroup(groupId);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects not associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <returns><see cref="IEnumerable{IUser}"/></returns>
        public IEnumerable<IUser> GetAllNotInGroup(int groupId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                return repository.GetAllNotInGroup(groupId);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetProfile(id);
            }
        }

        /// <summary>
        /// Gets a profile by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileByUserName(string username)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetProfile(username);
            }
        }

        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id">Id of the user to retrieve</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetUserById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.Get(id);
            }
        }

        public IEnumerable<IUser> GetUsersById(params int[] ids)
        {
            if (ids.Length <= 0) return Enumerable.Empty<IUser>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserRepository(uow);
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Replaces the same permission set for a single group to any number of entities
        /// </summary>
        /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified group.</remarks>
        /// <param name="groupId">Id of the group</param>
        /// <param name="permissions">Permissions as enumerable list of <see cref="char"/></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for. If nothing is specified all permissions are removed.</param>
        public void ReplaceUserGroupPermissions(int groupId, IEnumerable<char> permissions, params int[] entityIds)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                repository.ReplaceGroupPermissions(groupId, permissions, entityIds);
                uow.Commit();
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
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                repository.AssignGroupPermission(groupId, permission, entityIds);
                uow.Commit();
            }
        }
        
        /// <summary>
        /// Gets all UserGroups or those specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserGroups to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserGroup"/></returns>
        public IEnumerable<IUserGroup> GetAllUserGroups(params int[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                return repository.GetAll(ids).OrderBy(x => x.Name);
            }
        }
        
        public IEnumerable<IUserGroup> GetUserGroupsByAlias(params string[] aliases)
        {
            if (aliases.Length == 0) return Enumerable.Empty<IUserGroup>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                var query = Query<IUserGroup>.Builder.Where(x => aliases.SqlIn(x.Alias));
                var contents = repository.GetByQuery(query);
                return contents.ToArray();
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

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                var query = Query<IUserGroup>.Builder.Where(x => x.Alias == alias);
                var contents = repository.GetByQuery(query);
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
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                return repository.Get(id);
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
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(SavingUserGroup, this, new SaveEventArgs<IUserGroup>(userGroup)))
                {
                    uow.Commit();
                    return;
                }

                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                repository.AddOrUpdateGroupWithUsers(userGroup, userIds);
                
                uow.Commit();

                if (raiseEvents)
                    uow.Events.Dispatch(SavedUserGroup, this, new SaveEventArgs<IUserGroup>(userGroup, false));
            }
        }

        /// <summary>
        /// Deletes a UserGroup
        /// </summary>
        /// <param name="userGroup">UserGroup to delete</param>
        public void DeleteUserGroup(IUserGroup userGroup)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(DeletingUserGroup, this, new DeleteEventArgs<IUserGroup>(userGroup)))
                {
                    uow.Commit();
                    return;
                }
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                repository.Delete(userGroup);
                uow.Commit();
                uow.Events.Dispatch(DeletedUserGroup, this, new DeleteEventArgs<IUserGroup>(userGroup, false));
            }
        }

        /// <summary>
        /// Removes a specific section from all users
        /// </summary>
        /// <remarks>This is useful when an entire section is removed from config</remarks>
        /// <param name="sectionAlias">Alias of the section to remove</param>
        public void DeleteSectionFromAllUserGroups(string sectionAlias)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                var assignedGroups = repository.GetGroupsAssignedToSection(sectionAlias);
                foreach (var group in assignedGroups)
                {
                    //now remove the section for each user and commit
                    group.RemoveAllowedSection(sectionAlias);
                    repository.AddOrUpdate(group);
                }

                uow.Commit();
                //TODO: Events?
            }
        }        

        /// <summary>
        /// Get permissions set for a user and node Id
        /// </summary>
        /// <param name="user">User to retrieve permissions for</param>
        /// <param name="nodeIds">Specifiying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds)
        {
            var result = new List<EntityPermission>();
            foreach (var group in user.Groups)
            {
                //TODO: This may perform horribly :/ 
                foreach (var permission in GetPermissions(group.Alias, false, nodeIds))
                {
                    AddOrAmendPermissionList(result, permission);
                }
            }

            return result;
        }

        /// <summary>
        /// Get permissions set for a group and node Id
        /// </summary>
        /// <param name="groupAlias">Group to retrieve permissions for</param>
        /// <param name="directlyAssignedOnly">
        /// Flag indicating if we want to get just the permissions directly assigned for the group and path, 
        /// or fall back to the group's default permissions when nothing is directly assigned
        /// </param>
        /// <param name="nodeIds">Specifiying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public IEnumerable<EntityPermission> GetPermissions(string groupAlias, bool directlyAssignedOnly, params int[] nodeIds)
        {   
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                var group = repository.Get(groupAlias);
                return GetPermissionsInternal(repository, group, directlyAssignedOnly, nodeIds);
            }
        }

        /// <summary>
        /// Get permissions set for a group and optional node Ids
        /// </summary>
        /// <param name="group">Group to retrieve permissions for</param>
        /// <param name="directlyAssignedOnly">
        /// Flag indicating if we want to get just the permissions directly assigned for the group and path, 
        /// or fall back to the group's default permissions when nothing is directly assigned
        /// </param>
        /// <param name="nodeIds">Specifiying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public IEnumerable<EntityPermission> GetPermissions(IUserGroup group, bool directlyAssignedOnly, params int[] nodeIds)
        {
            if (group == null) throw new ArgumentNullException("group");
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateUserGroupRepository(uow);
                return GetPermissionsInternal(repository, group, directlyAssignedOnly, nodeIds);
            }
        }

        private IEnumerable<EntityPermission> GetPermissionsInternal(IUserGroupRepository repository, IUserGroup group, bool directlyAssignedOnly, params int[] nodeIds)
        {
            var explicitPermissions = repository.GetPermissionsForEntities(group.Id, nodeIds);
            var result = new List<EntityPermission>(explicitPermissions);

            // If requested, and no permissions are assigned to a particular node, then we will fill in those permissions with the group's defaults
            if (directlyAssignedOnly == false)
            {
                var missingIds = nodeIds.Except(result.Select(x => x.EntityId)).ToList();
                if (missingIds.Any())
                {
                    result.AddRange(missingIds
                        .Select(i => new EntityPermission(i, group.Permissions.ToArray())));
                }
            }
            return result;
        }

        /// <summary>
        /// For an existing list of <see cref="EntityPermission"/>, takes a new <see cref="EntityPermission"/> and aggregates it.
        /// If a permission for the entity associated with the new permission already exists, it's updated with those permissions to create a distinct, most permissive set.
        /// If it doesn't, it's added to the list.
        /// </summary>
        /// <param name="permissions">List of already found permissions</param>
        /// <param name="groupPermission">New permission to aggregate</param>
        private void AddOrAmendPermissionList(IList<EntityPermission> permissions, EntityPermission groupPermission)
        {
            //TODO: Fix the performance of this, we need to use things like HashSet and equality checkers, we are iterating too much

            var existingPermission = permissions.FirstOrDefault(x => x.EntityId == groupPermission.EntityId);
            if (existingPermission != null)
            {
                existingPermission.AddAdditionalPermissions(groupPermission.AssignedPermissions);
            }
            else
            {
                permissions.Add(groupPermission);
            }
        }

        /// <summary>
        /// Gets the permissions for the provided user and path
        /// </summary>
        /// <param name="user">User to check permissions for</param>
        /// <param name="path">Path to check permissions for</param>
        /// <returns>String indicating permissions for provided user and path</returns>
        public string GetPermissionsForPath(IUser user, string path)
        {   
            var assignedPermissions = GetPermissionsForGroupsAndPath(user.Groups.Select(x => x.Alias), path);
            return GetAggregatePermissions(assignedPermissions);
        }

        /// <summary>
        /// Retrieves the permissions assigned to each group for a given path
        /// </summary>
        /// <param name="groups">List of groups associated with the user</param>
        /// <param name="path">Path to check permissions for</param>
        /// <returns>List of strings indicating permissions for each groups</returns>
        private IEnumerable<string> GetPermissionsForGroupsAndPath(IEnumerable<string> groups, string path)
        {
            return groups
                .Select(g => GetPermissionsForPath(g, path, directlyAssignedOnly: false))
                .ToList();
        }

        /// <summary>
        /// Aggregates a set of permissions strings to return a unique permissions string containing the most permissive set
        /// </summary>
        /// <param name="assignedPermissions">List of permission strings</param>
        /// <returns>Single permission string</returns>
        private static string GetAggregatePermissions(IEnumerable<string> assignedPermissions)
        {
            return string.Join(string.Empty, assignedPermissions
                .SelectMany(s => s.ToCharArray())
                .Distinct());
        }

        /// <summary>
        /// Gets the permissions for the provided group and path
        /// </summary>
        /// <param name="groupAlias">User to check permissions for</param>
        /// <param name="path">Path to check permissions for</param>
        /// <param name="directlyAssignedOnly">
        /// Flag indicating if we want to get just the permissions directly assigned for the group and path, 
        /// or fall back to the group's default permissions when nothing is directly assigned
        /// </param>
        /// <returns>String indicating permissions for provided user and path</returns>
        public string GetPermissionsForPath(string groupAlias, string path, bool directlyAssignedOnly = true)
        {
            var nodeId = GetNodeIdFromPath(path);
            var permission = GetPermissions(groupAlias, directlyAssignedOnly, nodeId)
                .FirstOrDefault();
            return permission != null 
                ? string.Join(string.Empty, permission.AssignedPermissions)
                : string.Empty;
        }

        /// <summary>
        /// Gets the permissions for the provided group and path
        /// </summary>
        /// <param name="group">Group to check permissions for</param>
        /// <param name="path">Path to check permissions for</param>
        /// <param name="directlyAssignedOnly">
        /// Flag indicating if we want to get just the permissions directly assigned for the group and path, 
        /// or fall back to the group's default permissions when nothing is directly assigned
        /// </param>
        /// <returns>String indicating permissions for provided user and path</returns>
        public string GetPermissionsForPath(IUserGroup group, string path, bool directlyAssignedOnly = true)
        {
            var nodeId = GetNodeIdFromPath(path);
            var permission = GetPermissions(group, directlyAssignedOnly, nodeId)
                .FirstOrDefault();
            return permission != null
                ? string.Join(string.Empty, permission.AssignedPermissions)
                : string.Empty;
        }


        /// <summary>
        /// Parses a path to find the lowermost node id
        /// </summary>
        /// <param name="path">Path as string</param>
        /// <returns>Node id</returns>
        private static int GetNodeIdFromPath(string path)
        {
            return path.Contains(",")
                ? int.Parse(path.Substring(path.LastIndexOf(",", StringComparison.Ordinal) + 1))
                : int.Parse(path);
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
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserGroup>> SavingUserGroup;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserGroup>> SavedUserGroup;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserGroup>> DeletingUserGroup;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserGroup>> DeletedUserGroup;
    }
}
