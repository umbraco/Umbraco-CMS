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

        public UserService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();

                var types = repository.GetAll().Select(x => x.Alias).ToArray();

                if (types.Any() == false)
                {
                    throw new EntityNotFoundException("No member types could be resolved"); // causes rollback
                }

                uow.Complete();

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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var exists = repository.Exists(username);
                uow.Complete();
                return exists;
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

            User user;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var loginExists = uow.Database.ExecuteScalar<int>("SELECT COUNT(id) FROM umbracoUser WHERE userLogin = @Login", new { Login = username }) != 0;
                if (loginExists)
                    throw new ArgumentException("Login already exists"); // causes rollback

                user = new User(userType)
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
                //adding default sections content, media + translation
                user.AddAllowedSection(Constants.Applications.Content);
                user.AddAllowedSection(Constants.Applications.Media);
                user.AddAllowedSection(Constants.Applications.Translation);

                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(user), this))
                    return user;

                repository.AddOrUpdate(user);
                uow.Complete();
            }

            SavedUser.RaiseEvent(new SaveEventArgs<IUser>(user, false), this);
            return user;
        }

        /// <summary>
        /// Gets a User by its integer id
        /// </summary>
        /// <param name="id"><see cref="System.int"/> Id</param>
        /// <returns><see cref="IUser"/></returns>
        public IUser GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var user = repository.Get(id);
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var query = repository.Query.Where(x => x.Email.Equals(email));
                var user = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var query = repository.Query.Where(x => x.Username.Equals(username));
                var user = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
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

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IUserRepository>();
                    repository.Delete(user);
                    uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                try
                {
                    repository.AddOrUpdate(entity);
                    uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                foreach (var member in entities)
                {
                    repository.AddOrUpdate(member);
                }
                //commit the whole lot in one go
                uow.Complete();
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
        public IEnumerable<IUser> FindByEmail(string emailStringToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var query = repository.Query;

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

                var users = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Email);
                uow.Complete();
                return users;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var query = repository.Query;

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

                var users = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Username);
                uow.Complete();
                return users;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                IQuery<IUser> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = repository.Query;
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
                        query = repository.Query.Where(x => x.IsLockedOut);
                        break;
                    case MemberCountType.Approved:
                        query = repository.Query.Where(x => x.IsApproved);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("countType");
                }

                var count = repository.GetCountByQuery(query);
                uow.Complete();
                return count;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var users = repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Username);
                uow.Complete();
                return users;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var user = repository.Get(id);
                uow.Complete();
                return user;
            }
        }

        /// <summary>
        /// Replaces the same permission set for a single user to any number of entities
        /// </summary>
        /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified user.</remarks>
        /// <param name="userId">Id of the user</param>
        /// <param name="permissions">Permissions as enumerable list of <see cref="char"/></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for. If nothing is specified all permissions are removed.</param>
        public void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                repository.ReplaceUserPermissions(userId, permissions, entityIds);
                uow.Complete();
            }
        }

        /// <summary>
        /// Assigns the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="permission"></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for</param>
        public void AssignUserPermission(int userId, char permission, params int[] entityIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                repository.AssignUserPermission(userId, permission, entityIds);
                uow.Complete();
            }
        }

        /// <summary>
        /// Gets all UserTypes or thosed specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserTypes to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserType"/></returns>
        public IEnumerable<IUserType> GetAllUserTypes(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                var types = repository.GetAll(ids);
                uow.Complete();
                return types;
            }
        }

        /// <summary>
        /// Gets a UserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByAlias(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                var query = repository.QueryFactory.Create<IUserType>().Where(x => x.Alias == alias);
                var type = repository.GetByQuery(query).SingleOrDefault();
                uow.Complete();
                return type;
            }
        }

        /// <summary>
        /// Gets a UserType by its Id
        /// </summary>
        /// <param name="id">Id of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                var type = repository.Get(id);                
                uow.Complete();
                return type;
            }
        }

        /// <summary>
        /// Gets a UserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByName(string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                var query = repository.QueryFactory.Create<IUserType>().Where(x => x.Name == name);
                var type = repository.GetByQuery(query).SingleOrDefault();
                uow.Complete();
                return type;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                repository.AddOrUpdate(userType);
                uow.Complete();
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserTypeRepository>();
                repository.Delete(userType);
                uow.Complete();
            }

            DeletedUserType.RaiseEvent(new DeleteEventArgs<IUserType>(userType, false), this);
        }

        /// <summary>
        /// Removes a specific section from all users
        /// </summary>
        /// <remarks>This is useful when an entire section is removed from config</remarks>
        /// <param name="sectionAlias">Alias of the section to remove</param>
        public void DeleteSectionFromAllUsers(string sectionAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var assignedUsers = repository.GetUsersAssignedToSection(sectionAlias);
                foreach (var user in assignedUsers)
                {
                    //now remove the section for each user and commit
                    user.RemoveAllowedSection(sectionAlias);
                    repository.AddOrUpdate(user);
                }
                uow.Complete();
            }
        }
        
        /// <summary>
        /// Add a specific section to all users or those specified as parameters
        /// </summary>
        /// <remarks>This is useful when a new section is created to allow specific users accessing it</remarks>
        /// <param name="sectionAlias">Alias of the section to add</param>
        /// <param name="userIds">Specifiying nothing will add the section to all user</param>
        public void AddSectionToAllUsers(string sectionAlias, params int[] userIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                IEnumerable<IUser> users;
                if (userIds.Any())
                {
                    users = repository.GetAll(userIds);
                }
                else
                {
                    users = repository.GetAll();
                }
                foreach (var user in users.Where(u => !u.AllowedSections.InvariantContains(sectionAlias)))
                {
                    //now add the section for each user and commit
                    user.AddAllowedSection(sectionAlias);
                    repository.AddOrUpdate(user);
                }
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IUserRepository>();
                var explicitPermissions = repository.GetUserPermissionsForEntities(user.Id, nodeIds);

                //if no permissions are assigned to a particular node then we will fill in those permissions with the user's defaults
                var result = new List<EntityPermission>(explicitPermissions);
                var missingIds = nodeIds.Except(result.Select(x => x.EntityId));
                foreach (var id in missingIds)
                {
                    if (id == -1 && user.DefaultPermissions.Any() == false)
                    {
                        // exception to everything. If default cruds is empty and we're on root node; allow browse of root node
                        result.Add(new EntityPermission(user.Id, id, user.DefaultPermissions.ToArray()));
                    }
                    else
                    {
                        //use the user's user type permissions
                        result.Add(new EntityPermission(user.Id, id, user.DefaultPermissions.ToArray()));
                    }                    
                }

                uow.Complete();
                return result;
            }
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
    }
}
