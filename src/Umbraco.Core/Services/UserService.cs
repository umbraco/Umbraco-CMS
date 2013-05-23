using System.Globalization;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
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

        public IMembershipUser CreateUser(string name, string login, string password, IUserType userType)
        {
            return CreateUser(name, login, password, userType.Id);
        }

        public IMembershipUser CreateUser(string name, string login, string password, int userTypeId)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateUserRepository(uow))
            {
                var dto = new UserDto
                              {
                                  UserLanguage = Umbraco.Core.Configuration.GlobalSettings.DefaultUILanguage,
                                  Login = login,
                                  UserName = name,
                                  Email = string.Empty,
                                  Type = short.Parse(userTypeId.ToString(CultureInfo.InvariantCulture)),
                                  ContentStartId = -1,
                                  MediaStartId = -1,
                                  Password = password
                              };

                //TODO Check if user exists and throw?
                uow.Database.Insert(dto);

                return new User(new UserType());
            }
        }

        #endregion
    }
}