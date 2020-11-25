using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class ExternalLoginService : ScopeRepositoryService, IExternalLoginService
    {
        private readonly IExternalLoginRepository _externalLoginRepository;

        public ExternalLoginService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginRepository externalLoginRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
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
        public void Save(IIdentityUserLogin login)
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
