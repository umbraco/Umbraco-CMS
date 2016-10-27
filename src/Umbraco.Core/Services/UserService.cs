using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Security;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/>, <see cref="IMembershipUser"/> and eventually Backoffice Users.
    /// </summary>
    public class UserService : RepositoryService, IUserService
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
        /// Gets the default MemberType alias
        /// </summary>
        /// <remarks>By default we'll return the 'writer', but we need to check it exists. If it doesn't we'll 
        /// return the first type that is not an admin, otherwise if there's only one we will return that one.</remarks>
        /// <returns>Alias of the default MemberType</returns>
        public string GetDefaultMemberType()
        {
            using (var repository = RepositoryFactory.CreateUserTypeRepository(UowProvider.GetUnitOfWork()))
            {
                var types = repository.GetAll().Select(x => x.Alias).ToArray();

                if (types.Any() == false)
                {
                    throw new EntityNotFoundException("No member types could be resolved");
                }

                if (types.InvariantContains("writer"))
                {
                    return types.First(x => x.InvariantEquals("writer"));
                }
                
                if (types.Length == 1)
                {
                    return types.First();
                }

                //first that is not admin
                return types.First(x => x.InvariantEquals("admin") == false);
            }
        }

        /// <summary>
        /// Checks if a User with the username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns><c>True</c> if the User exists otherwise <c>False</c></returns>
        public bool Exists(string username)
        {
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Exists(username);
            }
        }

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <remarks>The user will be saved in the database and returned with an Id</remarks>
        /// <param name="username">Username of the user to create</param>
        /// <param name="email">Email of the user to create</param>
        /// <param name="userType"><see cref="IUserType"/> which the User should be based on</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser CreateUserWithIdentity(string username, string email, IUserType userType)
        {
            return CreateUserWithIdentity(username, email, "", userType);
        }

        /// <summary>
        /// Creates and persists a new <see cref="IUser"/>
        /// </summary>
        /// <param name="username">Username of the <see cref="IUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Alias of the Type</param>
        /// <returns><see cref="IUser"/></returns>
        IUser IMembershipMemberService<IUser>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias)
        {
            var userType = GetUserTypeByAlias(memberTypeAlias);
            if (userType == null)
            {
                throw new EntityNotFoundException("The user type " + memberTypeAlias + " could not be resolved");
            }

            return CreateUserWithIdentity(username, email, passwordValue, userType);
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned 
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="userType">MemberType the Member should be based on</param>
        /// <returns><see cref="IUser"/></returns>
        private IUser CreateUserWithIdentity(string username, string email, string passwordValue, IUserType userType)
        {
            if (userType == null) throw new ArgumentNullException("userType");

            //TODO: PUT lock here!!

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                var loginExists = uow.Database.ExecuteScalar<int>("SELECT COUNT(id) FROM umbracoUser WHERE userLogin = @Login", new { Login = username }) != 0;
                if (loginExists)
                    throw new ArgumentException("Login already exists");

                var user = new User(userType)
                {
                    DefaultToLiveEditing = false,
                    Email = email,
                    Language = Configuration.GlobalSettings.DefaultUILanguage,
                    Name = username,
                    RawPasswordValue = passwordValue,                    
                    Username = username,
                    StartContentId = -1,
                    StartMediaId = -1,
                    IsLockedOut = false,
                    IsApproved = true
                };

                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(user), this))
                    return user;

                repository.AddOrUpdate(user);
                uow.Commit();

                SavedUser.RaiseEvent(new SaveEventArgs<IUser>(user, false), this);

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
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                var user = repository.Get((int)id);

                return user;
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
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUser>.Builder.Where(x => x.Email.Equals(email));
                var user = repository.GetByQuery(query).FirstOrDefault();

                return user;
            }
        }
        
        /// <summary>
        /// Get an <see cref="IUser"/> by username
        /// </summary>
        /// <param name="username">Username to use for retrieval</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetByUsername(string username)
        {
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUser>.Builder.Where(x => x.Username.Equals(username));
                var user = repository.GetByQuery(query).FirstOrDefault();
                return user;
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
            //can't rename if it's going to take up too many chars
            if (membershipUser.Username.Length + 9 <= 125)
            {
                membershipUser.Username = DateTime.Now.ToString("yyyyMMdd") + "_" + membershipUser.Username;
            }
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
                if (DeletingUser.IsRaisedEventCancelled(new DeleteEventArgs<IUser>(user), this))
                    return;

                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateUserRepository(uow))
                {
                    repository.Delete(user);
                    uow.Commit();
                }

                DeletedUser.RaiseEvent(new DeleteEventArgs<IUser>(user, false), this);
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
            if (raiseEvents)
            {
                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(entity), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                repository.AddOrUpdate(entity);
                try
                {
                    uow.Commit();
                }
                catch (DbException ex)
                {
                    //Special case, if we are upgrading and an exception occurs, just continue
                    if (IsUpgrading == false) throw;

                    Logger.WarnWithException<UserService>("An error occurred attempting to save a user instance during upgrade, normally this warning can be ignored", ex);
                    return;
                }
            }

            if (raiseEvents)
                SavedUser.RaiseEvent(new SaveEventArgs<IUser>(entity, false), this);
        }

        /// <summary>
        /// Saves a list of <see cref="IUser"/> objects
        /// </summary>
        /// <param name="entities"><see cref="IEnumerable{IUser}"/> to save</param>
        /// <param name="raiseEvents">Optional parameter to raise events. 
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IEnumerable<IUser> entities, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(entities), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                foreach (var member in entities)
                {
                    repository.AddOrUpdate(member);
                }
                //commit the whole lot in one go
                uow.Commit();
            }

            if (raiseEvents)
                SavedUser.RaiseEvent(new SaveEventArgs<IUser>(entities, false), this);
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
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
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                IQuery<IUser> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = new Query<IUser>();
                        return repository.Count(query);
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
                        query =
                            Query<IUser>.Builder.Where(
                                x => x.IsLockedOut);
                        return repository.GetCountByQuery(query);
                    case MemberCountType.Approved:
                        query =
                            Query<IUser>.Builder.Where(
                                x => x.IsApproved);
                        return repository.GetCountByQuery(query);
                    default:
                        throw new ArgumentOutOfRangeException("countType");
                }
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                return repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Username);
            }
        }

        internal IEnumerable<IUser> GetNextUsers(int id, int count)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = (UserRepository) RepositoryFactory.CreateUserRepository(uow))
            {
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
            var user = GetUserById(id);
            return user.ProfileData;
        }

        /// <summary>
        /// Gets a profile by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileByUserName(string username)
        {
            var user = GetByUsername(username);
            return user.ProfileData;
        }

        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id">Id of the user to retrieve</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetUserById(int id)
        {
            using (var repository = RepositoryFactory.CreateUserRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                repository.ReplaceGroupPermissions(groupId, permissions, entityIds);
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                repository.AssignGroupPermission(groupId, permission, entityIds);
            }
        }

        /// <summary>
        /// Gets all UserTypes or those specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserTypes to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserType"/></returns>
        public IEnumerable<IUserType> GetAllUserTypes(params int[] ids)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserTypeRepository(uow))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a UserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByAlias(string alias)
        {
            using (var repository = RepositoryFactory.CreateUserTypeRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserType>.Builder.Where(x => x.Alias == alias);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets a UserType by its Id
        /// </summary>
        /// <param name="id">Id of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeById(int id)
        {
            using (var repository = RepositoryFactory.CreateUserTypeRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);                
            }
        }

        /// <summary>
        /// Gets a UserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByName(string name)
        {
            using (var repository = RepositoryFactory.CreateUserTypeRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserType>.Builder.Where(x => x.Name == name);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Saves a UserType
        /// </summary>
        /// <param name="userType">UserType to save</param>
        /// <param name="raiseEvents">Optional parameter to raise events. 
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void SaveUserType(IUserType userType, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUserType.IsRaisedEventCancelled(new SaveEventArgs<IUserType>(userType), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.AddOrUpdate(userType);
                uow.Commit();
            }

            if (raiseEvents)
                SavedUserType.RaiseEvent(new SaveEventArgs<IUserType>(userType, false), this);
        }

        /// <summary>
        /// Deletes a UserType
        /// </summary>
        /// <param name="userType">UserType to delete</param>
        public void DeleteUserType(IUserType userType)
        {
            if (DeletingUserType.IsRaisedEventCancelled(new DeleteEventArgs<IUserType>(userType), this))
                return;

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.Delete(userType);
                uow.Commit();
            }

            DeletedUserType.RaiseEvent(new DeleteEventArgs<IUserType>(userType, false), this);
        }

        /// <summary>
        /// Gets all UserGroups or those specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserGroups to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserGroup"/></returns>
        public IEnumerable<IUserGroup> GetAllUserGroups(params int[] ids)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                return repository.GetAll(ids).OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Gets all UserGroups for a given user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>An enumerable list of <see cref="IUserGroup"/></returns>
        public IEnumerable<IUserGroup> GetGroupsForUser(int userId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                return repository.GetGroupsForUser(userId);
            }
        }

        /// <summary>
        /// Gets a UserGroup by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserGroup to retrieve</param>
        /// <returns><see cref="IUserGroup"/></returns>
        public IUserGroup GetUserGroupByAlias(string alias)
        {
            using (var repository = RepositoryFactory.CreateUserGroupRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserGroup>.Builder.Where(x => x.Alias == alias);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets a UserGroup by its Id
        /// </summary>
        /// <param name="id">Id of the UserGroup to retrieve</param>
        /// <returns><see cref="IUserGroup"/></returns>
        public IUserGroup GetUserGroupById(int id)
        {
            using (var repository = RepositoryFactory.CreateUserGroupRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a UserGroup by its Name
        /// </summary>
        /// <param name="name">Name of the UserGroup to retrieve</param>
        /// <returns><see cref="IUserGroup"/></returns>
        public IUserGroup GetUserGroupByName(string name)
        {
            using (var repository = RepositoryFactory.CreateUserGroupRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserGroup>.Builder.Where(x => x.Name == name);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Saves a UserGroup
        /// </summary>
        /// <param name="userGroup">UserGroup to save</param>
        /// <param name="updateUsers">Flag for whether to update the list of users in the group</param>
        /// <param name="userIds">List of user Ids</param>
        /// <param name="raiseEvents">Optional parameter to raise events. 
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void SaveUserGroup(IUserGroup userGroup, bool updateUsers = false, int[] userIds = null, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUserGroup.IsRaisedEventCancelled(new SaveEventArgs<IUserGroup>(userGroup), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                repository.AddOrUpdate(userGroup);
                uow.Commit();

                if (updateUsers)
                {
                    repository.RemoveAllUsersFromGroup(userGroup.Id);
                    repository.AddUsersToGroup(userGroup.Id, userIds);
                }
            }

            if (raiseEvents)
                SavedUserGroup.RaiseEvent(new SaveEventArgs<IUserGroup>(userGroup, false), this);
        }

        /// <summary>
        /// Deletes a UserGroup
        /// </summary>
        /// <param name="userGroup">UserGroup to delete</param>
        public void DeleteUserGroup(IUserGroup userGroup)
        {
            if (DeletingUserGroup.IsRaisedEventCancelled(new DeleteEventArgs<IUserGroup>(userGroup), this))
                return;

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                repository.Delete(userGroup);
                uow.Commit();
            }

            DeletedUserGroup.RaiseEvent(new DeleteEventArgs<IUserGroup>(userGroup, false), this);
        }

        /// <summary>
        /// Removes a specific section from all users
        /// </summary>
        /// <remarks>This is useful when an entire section is removed from config</remarks>
        /// <param name="sectionAlias">Alias of the section to remove</param>
        public void DeleteSectionFromAllUserGroups(string sectionAlias)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                var assignedGroups = repository.GetGroupsAssignedToSection(sectionAlias);
                foreach (var group in assignedGroups)
                {
                    //now remove the section for each user and commit
                    group.RemoveAllowedSection(sectionAlias);
                    repository.AddOrUpdate(group);
                }

                uow.Commit();
            }
        }

        /// <summary>
        /// Add a specific section to all user groups or those specified as parameters
        /// </summary>
        /// <remarks>This is useful when a new section is created to allow specific user groups to  access it</remarks>
        /// <param name="sectionAlias">Alias of the section to add</param>
        /// <param name="groupIds">Specifiying nothing will add the section to all user</param>
        public void AddSectionToAllUserGroups(string sectionAlias, params int[] groupIds)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserGroupRepository(uow))
            {
                IEnumerable<IUserGroup> groups;
                if (groupIds.Any())
                {
                    groups = repository.GetAll(groupIds);
                }
                else
                {
                    groups = repository.GetAll();
                }
                foreach (var group in groups.Where(g => g.AllowedSections.InvariantContains(sectionAlias) == false))
                {
                    //now add the section for each group and commit
                    group.AddAllowedSection(sectionAlias);
                    repository.AddOrUpdate(group);
                }

                uow.Commit();
            }
        }    

        /// <summary>
        /// Get permissions set for a user and optional node ids
        /// </summary>
        /// <remarks>If no permissions are found for a particular entity then the user's default permissions will be applied</remarks>
        /// <param name="user">User to retrieve permissions for</param>
        /// <param name="nodeIds">Specifiying nothing will return all user permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                // TODO: rework (to use groups - currently defaulting to user type)

                //if no permissions are assigned to a particular node then we will fill in those permissions with the user's defaults
                var result = new List<EntityPermission>();
                var missingIds = nodeIds.Except(result.Select(x => x.EntityId));
                foreach (var id in missingIds)
                {
                    result.Add(
                        new EntityPermission(
                            id,
                            user.DefaultPermissions.ToArray()));
                }

                return result;
            }
        }

        /// <summary>
        /// Get permissions set for a group and optional node ids
        /// </summary>
        /// <param name="group">Group to retrieve permissions for</param>
        /// <param name="nodeIds">Specifiying nothing will return all permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        public IEnumerable<EntityPermission> GetPermissions(IUserGroup group, params int[] nodeIds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get permissions set for a user and optional node ids
        /// </summary>
        /// <param name="repository">User repository</param>
        /// <param name="user">User to retrieve permissions for</param>
        /// <param name="nodeIds">Specifiying nothing will return all user permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        private IEnumerable<EntityPermission> GetPermissions(IUserRepository repository, IUser user, params int[] nodeIds)
        {
            var result = new List<EntityPermission>();

            // TODO: rework to groups first, then user type

            // If no permissions are assigned to a particular node then, we will fill in those permissions with the user's defaults
            var missingIds = nodeIds.Except(result.Select(x => x.EntityId));
            foreach (var id in missingIds)
            {
                result.Add(
                    new EntityPermission(
                        id,
                        user.DefaultPermissions.ToArray()));
            }

            // Add or amend the existing permissions based on these
            foreach (var group in user.Groups)
            {
                var groupPermissions = GetPermissions(group, nodeIds).ToList();
                foreach (var groupPermission in groupPermissions)
                {
                    // Add group permission, ensuring we keep a unique value for the entity Id in the list
                    AddOrAmendPermission(result, groupPermission);
                }
            }

            return result;
        }

        private void AddOrAmendPermission(IList<EntityPermission> permissions, EntityPermission groupPermission)
        {
            var existingPermission = permissions
                .SingleOrDefault(x => x.EntityId == groupPermission.EntityId);
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
            var nodeId = GetNodeIdFromPath(path);

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateUserRepository(uow))
            {
                // Check for permissions directly assigned to the node for the user (if we have any, they take priority
                // over anything defined on groups; we'll take the special "null" action as not being defined
                // as this is set if someone sets permissions on a user and then removes them)
                var permissions = GetPermissions(repository, user, nodeId)
                    .Where(IsNotNullActionPermission)
                    .ToList();
                var gotDirectlyAssignedUserPermissions = permissions.Any();

                // If none found, and checking groups, get permissions from the groups
                if (gotDirectlyAssignedUserPermissions == false)
                {
                    foreach (var group in user.Groups)
                    {
                        var groupPermissions = GetPermissions(group, nodeId).ToList();
                        permissions.AddRange(groupPermissions);
                    }
                }

                // If none found directly on the user, get those defined on the user type
                if (gotDirectlyAssignedUserPermissions == false)
                {
                    var typePermissions = GetPermissions(repository, user, nodeId).ToList();
                    permissions.AddRange(typePermissions);
                }

                // Extract the net permissions from the path from the set of permissions found
                string assignedPermissions;
                if (TryGetAssignedPermissionsForNode(permissions, nodeId, out assignedPermissions))
                {
                    return assignedPermissions;
                }

                // Exception to everything. If default cruds is empty and we're on root node; allow browse of root node
                if (path == "-1")
                {
                    return "F";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the permissions for the provided group and path
        /// </summary>
        /// <param name="group">User to check permissions for</param>
        /// <param name="path">Path to check permissions for</param>
        /// <returns>String indicating permissions for provided user and path</returns>
        public string GetPermissionsForPath(IUserGroup group, string path)
        {
            var nodeId = GetNodeIdFromPath(path);

            var permissions = GetPermissions(group).ToList();

            string assignedPermissions;
            TryGetAssignedPermissionsForNode(permissions, nodeId, out assignedPermissions);
            return assignedPermissions;
        }

        private static int GetNodeIdFromPath(string path)
        {
            return path.Contains(",")
                ? int.Parse(path.Substring(path.LastIndexOf(",", StringComparison.Ordinal) + 1))
                : int.Parse(path);
        }

        private static bool IsNotNullActionPermission(EntityPermission x)
        {
            const string NullActionChar = "-";
            return string.Join(string.Empty, x.AssignedPermissions) != NullActionChar;
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
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserType>> SavingUserType;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserType>> SavedUserType;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserType>> DeletingUserType;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserType>> DeletedUserType;

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
