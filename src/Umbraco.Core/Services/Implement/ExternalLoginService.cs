using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class ExternalLoginService : ScopeRepositoryService, IExternalLoginService
    {
        private readonly IExternalLoginRepository _externalLoginRepository;

        public ExternalLoginService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginRepository externalLoginRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _externalLoginRepository = externalLoginRepository;
        }

        /// <summary>
        /// Returns all user logins assigned
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<IIdentityUserLogin> GetAll(int userId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.UserId == userId))
                    .ToList(); // ToList is important here, must evaluate within uow! // ToList is important here, must evaluate within uow!
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.ProviderKey == login.ProviderKey && x.LoginProvider == login.LoginProvider))
                    .ToList(); // ToList is important here, must evaluate within uow! // ToList is important here, must evaluate within uow!
            }
        }

        /// <summary>
        /// Save user logins
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="logins"></param>
        public void SaveUserLogins(int userId, IEnumerable<UserLoginInfo> logins)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.SaveUserLogins(userId, logins);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes all user logins - normally used when a member is deleted
        /// </summary>
        /// <param name="userId"></param>
        public void DeleteUserLogins(int userId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.DeleteUserLogins(userId);
                scope.Complete();
            }
        }
    }
}
