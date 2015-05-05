using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class ExternalLoginService : RepositoryService, IExternalLoginService
    {
        public ExternalLoginService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        /// <summary>
        /// Returns all user logins assigned
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<IIdentityUserLogin> GetAll(int userId)
        {            
            using (var repo = RepositoryFactory.CreateExternalLoginRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetByQuery(new Query<IIdentityUserLogin>().Where(x => x.UserId == userId));
            }
        }

        /// <summary>
        /// Returns all logins matching the login info - generally there should only be one but in some cases 
        /// there might be more than one depending on if an adminstrator has been editing/removing members
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public IEnumerable<IIdentityUserLogin> Find(UserLoginInfo login)
        {
            using (var repo = RepositoryFactory.CreateExternalLoginRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetByQuery(new Query<IIdentityUserLogin>()
                    .Where(x => x.ProviderKey == login.ProviderKey && x.LoginProvider == login.LoginProvider));
            }
        }

        /// <summary>
        /// Save user logins
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="logins"></param>
        public void SaveUserLogins(int userId, IEnumerable<UserLoginInfo> logins)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateExternalLoginRepository(uow))
            {
                repo.SaveUserLogins(userId, logins);
                uow.Commit();
            }
        }

        /// <summary>
        /// Deletes all user logins - normally used when a member is deleted
        /// </summary>
        /// <param name="userId"></param>
        public void DeleteUserLogins(int userId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateExternalLoginRepository(uow))
            {
                repo.DeleteUserLogins(userId);
                uow.Commit();
            }
        }

        
    }
}