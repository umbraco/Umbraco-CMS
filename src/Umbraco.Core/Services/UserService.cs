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
    internal class UserService : IUserService
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

        public bool Exists(string username)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(username);
            }
        }

        public IUser CreateMember(string username, string email, string password, IUserType userType)
        {
            if (userType == null) throw new ArgumentNullException("userType");

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

                repository.AddOrUpdate(user);
                uow.Commit();

                return user;
            }
        }

        public IUser CreateMember(string username, string email, string password, string memberTypeAlias)
        {
            var userType = GetUserTypeByAlias(memberTypeAlias);
            if (userType == null)
            {
                throw new ArgumentException("The user type " + memberTypeAlias + " could not be resolved");
            }

            return CreateMember(username, email, password, userType);
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
                var query = Query<IUser>.Builder.Where(x => x.Username == login);
                return repository.GetByQuery(query).FirstOrDefault();
            }
        }

        public void Delete(IUser membershipUser)
        {
            if (UserDeleting.IsRaisedEventCancelled(new DeleteEventArgs<IUser>(membershipUser), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                repository.Delete(membershipUser);
                uow.Commit();
            }

            UserDeleted.RaiseEvent(new DeleteEventArgs<IUser>(membershipUser, false), this);
        }

        public void Save(IUser membershipUser, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (UserSaving.IsRaisedEventCancelled(new SaveEventArgs<IUser>(membershipUser), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                repository.AddOrUpdate(membershipUser);
                uow.Commit();
            }

            if (raiseEvents)
                UserSaved.RaiseEvent(new SaveEventArgs<IUser>(membershipUser, false), this);
        }

        public void Save(IEnumerable<IUser> members, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (UserSaving.IsRaisedEventCancelled(new SaveEventArgs<IUser>(members), this))
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
                UserSaved.RaiseEvent(new SaveEventArgs<IUser>(members, false), this);
        }

        public IEnumerable<IUser> FindMembersByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUser> FindMembersByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            throw new NotImplementedException();
        }

        public int GetMemberCount(MemberCountType countType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUser> GetAllMembers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
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
                if (UserTypeSaving.IsRaisedEventCancelled(new SaveEventArgs<IUserType>(userType), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.AddOrUpdate(userType);
                uow.Commit();
            }

            if (raiseEvents)
                UserTypeSaved.RaiseEvent(new SaveEventArgs<IUserType>(userType, false), this);
        }

        public void DeleteUserType(IUserType userType)
        {
            if (UserTypeDeleting.IsRaisedEventCancelled(new DeleteEventArgs<IUserType>(userType), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserTypeRepository(uow))
            {
                repository.Delete(userType);
                uow.Commit();
            }

            UserTypeDeleted.RaiseEvent(new DeleteEventArgs<IUserType>(userType, false), this);
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

        #endregion

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUser>> UserSaving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUser>> UserSaved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUser>> UserDeleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUser>> UserDeleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserType>> UserTypeSaving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IUserService, SaveEventArgs<IUserType>> UserTypeSaved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserType>> UserTypeDeleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IUserService, DeleteEventArgs<IUserType>> UserTypeDeleted;
    }
}