using System;
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

        #region Implementation of IUserService

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileById(int id)
        {
            var user = GetUserById(id);
            return new Profile(user.Id, user.Name);
        }

        public IProfile GetProfileByUserName(string username)
        {
            var user = GetUserByUserName(username);
            return new Profile(user.Id, user.Name);
        }

        public IUser GetUserByUserName(string username)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IUser>.Builder.Where(x => x.Username == username);
                return repository.GetByQuery(query).FirstOrDefault();
            }
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

        /// <summary>
        /// Savers changes to a user to the database
        /// </summary>
        /// <param name="user"></param>
        public void SaveUser(IUser user)
        {
            if (UserSaving.IsRaisedEventCancelled(new SaveEventArgs<IUser>(user), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                repository.AddOrUpdate(user);
                uow.Commit();
            }

            UserSaved.RaiseEvent(new SaveEventArgs<IUser>(user, false), this);
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
        /// Creates a new user for logging into the umbraco backoffice
        /// </summary>
        /// <param name="name"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="userType"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public IMembershipUser CreateMembershipUser(string name, string login, string password, IUserType userType, string email = "")
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                var loginExists = uow.Database.ExecuteScalar<int>("SELECT COUNT(id) FROM umbracoUser WHERE userLogin = @Login", new { Login = login }) != 0;
                if (loginExists)
                    throw new ArgumentException("Login already exists");

                var user = new User(userType)
                {
                    DefaultToLiveEditing = false,
                    Email = email,
                    Language = Umbraco.Core.Configuration.GlobalSettings.DefaultUILanguage,
                    Name = name,
                    Password = password,
                    DefaultPermissions = userType.Permissions,
                    Username = login,
                    StartContentId = -1,
                    StartMediaId = -1,
                    NoConsole = false,
                    IsApproved = true
                };

                repository.AddOrUpdate(user);
                uow.Commit();

                return user;
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
    }
}