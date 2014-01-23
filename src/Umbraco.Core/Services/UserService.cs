using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/>, <see cref="IMembershipUser"/> and eventually Backoffice Users.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public UserService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        { }

        public UserService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        { }

        public UserService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _uowProvider = provider;
        }

        #region Implementation of IMembershipUserService

        /// <summary>
        /// By default we'll return the 'writer', but we need to check it exists. If it doesn't we'll return the first type that is not an admin, otherwise if there's only one
        /// we will return that one.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultMemberType()
        {
            using (var repository = _repositoryFactory.CreateUserTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var types = repository.GetAll().Select(x => x.Alias).ToArray();

                if (types.Any() == false)
                {
                    throw new InvalidOperationException("No member types could be resolved");
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

        public bool Exists(string username)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(username);
            }
        }

        public IUser CreateMemberWithIdentity(string username, string email, string password, IUserType userType, bool raiseEvents = true)
        {
            if (userType == null) throw new ArgumentNullException("userType");

            //TODO: PUT lock here!!

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
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
                    Password = password,
                    DefaultPermissions = userType.Permissions,
                    Username = username,
                    StartContentId = -1,
                    StartMediaId = -1,
                    IsLockedOut = false,
                    IsApproved = true
                };

                if (raiseEvents)
                {
                    if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(user), this))
                        return user;
                }

                repository.AddOrUpdate(user);
                uow.Commit();

                if (raiseEvents)
                    SavedUser.RaiseEvent(new SaveEventArgs<IUser>(user, false), this);

                return user;
            }
        }

        public IUser CreateMemberWithIdentity(string username, string email, string password, string memberTypeAlias, bool raiseEvents = true)
        {
            var userType = GetUserTypeByAlias(memberTypeAlias);
            if (userType == null)
            {
                throw new ArgumentException("The user type " + memberTypeAlias + " could not be resolved");
            }

            return CreateMemberWithIdentity(username, email, password, userType);
        }

        public IUser GetById(object id)
        {
            if (id is int)
            {
                using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
                {
                    var user = repository.Get((int) id);

                    return user;
                }
            }

            return null;
        }

        public IUser GetByEmail(string email)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUser>.Builder.Where(x => x.Email.Equals(email));
                var user = repository.GetByQuery(query).FirstOrDefault();

                return user;
            }
        }

        public IUser GetByUsername(string login)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUser>.Builder.Where(x => x.Username.Equals(login));
                return repository.GetByQuery(query).FirstOrDefault();
            }
        }

        public void Delete(IUser membershipUser)
        {
            if (DeletingUser.IsRaisedEventCancelled(new DeleteEventArgs<IUser>(membershipUser), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                repository.Delete(membershipUser);
                uow.Commit();
            }

            DeletedUser.RaiseEvent(new DeleteEventArgs<IUser>(membershipUser, false), this);
        }

        public void Save(IUser membershipUser, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(membershipUser), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                repository.AddOrUpdate(membershipUser);
                uow.Commit();
            }

            if (raiseEvents)
                SavedUser.RaiseEvent(new SaveEventArgs<IUser>(membershipUser, false), this);
        }

        public void Save(IEnumerable<IUser> members, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUser.IsRaisedEventCancelled(new SaveEventArgs<IUser>(members), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                foreach (var member in members)
                {
                    repository.AddOrUpdate(member);                 
                }
                //commit the whole lot in one go
                uow.Commit();
            }

            if (raiseEvents)
                SavedUser.RaiseEvent(new SaveEventArgs<IUser>(members, false), this);
        }

        public IEnumerable<IUser> FindMembersByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
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

        public IEnumerable<IUser> FindMembersByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
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

        public int GetMemberCount(MemberCountType countType)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
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

        public IEnumerable<IUser> GetAllMembers(int pageIndex, int pageSize, out int totalRecords)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                return repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Username);
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
            return user;
        }

        public IProfile GetProfileByUserName(string login)
        {
            var user = GetByUsername(login);
            return user;
        }

        public IUser GetUserById(int id)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an IUserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByAlias(string alias)
        {
            using (var repository = _repositoryFactory.CreateUserTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserType>.Builder.Where(x => x.Alias == alias);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets an IUserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        public IUserType GetUserTypeByName(string name)
        {
            using (var repository = _repositoryFactory.CreateUserTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUserType>.Builder.Where(x => x.Name == name);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        public void SaveUserType(IUserType userType, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (SavingUserType.IsRaisedEventCancelled(new SaveEventArgs<IUserType>(userType), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.AddOrUpdate(userType);
                uow.Commit();
            }

            if (raiseEvents)
                SavedUserType.RaiseEvent(new SaveEventArgs<IUserType>(userType, false), this);
        }

        public void DeleteUserType(IUserType userType)
        {
            if (DeletingUserType.IsRaisedEventCancelled(new DeleteEventArgs<IUserType>(userType), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.Delete(userType);
                uow.Commit();
            }

            DeletedUserType.RaiseEvent(new DeleteEventArgs<IUserType>(userType, false), this);
        }

        /// <summary>
        /// This is useful for when a section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        public void DeleteSectionFromAllUsers(string sectionAlias)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                var assignedUsers = repository.GetUsersAssignedToSection(sectionAlias);
                foreach (var user in assignedUsers)
                {
                    //now remove the section for each user and commit
                    user.RemoveAllowedSection(sectionAlias);
                    repository.AddOrUpdate(user);
                }
                uow.Commit();
            }
        }

        /// <summary>
        /// Returns the user's applications that they are allowed to access
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUserSections(IUser user)
        {
            //TODO: We need to cache this result

            var uow = _uowProvider.GetUnitOfWork();
            var sql = new Sql();
            sql.Select("app").From<User2AppDto>()
                .Where<User2AppDto>(dto => dto.UserId == (int)user.Id);
            return uow.Database.Fetch<string>(sql);
        }

        /// <summary>
        /// Returns permissions for a given user for any number of nodes
        /// </summary>
        /// <param name="user"></param>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        /// <remarks>
        /// If no permissions are found for a particular entity then the user's default permissions will be applied
        /// </remarks>
        public IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                var explicitPermissions = repository.GetUserPermissionsForEntities(user.Id, nodeIds);

                //if no permissions are assigned to a particular node then we will fill in those permissions with the user's defaults
                var result = new List<EntityPermission>(explicitPermissions);
                var missingIds = nodeIds.Except(result.Select(x => x.EntityId));
                foreach (var id in missingIds)
                {
                    result.Add(
                        new EntityPermission(
                            user.Id,
                            id,
                            user.DefaultPermissions.ToArray()));
                }

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