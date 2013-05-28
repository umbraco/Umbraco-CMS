using System;
using System.Globalization;
using System.Linq;
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

        public UserService(RepositoryFactory repositoryFactory) : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {}

        public UserService(IDatabaseUnitOfWorkProvider provider) : this(provider, new RepositoryFactory())
        {}

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
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetProfileById(id);
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
        /// Creates a new user for logging into the umbraco backoffice
        /// </summary>
        /// <param name="name"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="userType"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public IMembershipUser CreateUser(string name, string login, string password, IUserType userType, string email = "")
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
                                   Permissions = userType.Permissions,
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
    }
}