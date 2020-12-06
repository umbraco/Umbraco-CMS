using System;
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

        /// <inheritdoc />
        public IEnumerable<IIdentityUserLogin> GetAll(int userId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.UserId == userId))
                    .ToList();
            }
        }

        [Obsolete("Use the overload specifying loginProvider and providerKey instead")]
        public IEnumerable<IIdentityUserLogin> Find(UserLoginInfo login)
        {
            return Find(login.LoginProvider, login.ProviderKey);
        }

        /// <inheritdoc />
        public IEnumerable<IIdentityUserLogin> Find(string loginProvider, string providerKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>()
                    .Where(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider))
                    .ToList();
            }
        }

        [Obsolete("Use the Save method instead")]
        public void SaveUserLogins(int userId, IEnumerable<UserLoginInfo> logins)
        {
            Save(userId, logins.Select(x => new ExternalLogin(x.LoginProvider, x.ProviderKey)));
        }

        /// <inheritdoc />
        public void Save(int userId, IEnumerable<IExternalLogin> logins)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.Save(userId, logins);
                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void Save(IIdentityUserLoginExtended login)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.Save(login);
                scope.Complete();
            }
        }

        /// <inheritdoc />
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
