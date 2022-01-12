using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services.Implement
{

      public class ExternalLoginWithKeyService : RepositoryService, IExternalLoginWithKeyService
    {
        private readonly IExternalLoginWithKeyRepository _externalLoginRepository;

        public ExternalLoginWithKeyService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginWithKeyRepository externalLoginRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _externalLoginRepository = externalLoginRepository;
        }

        /// <inheritdoc />
        public IEnumerable<IIdentityUserLogin> GetExternalLogins(Guid userOrMemberKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.Key == userOrMemberKey))
                    .ToList();
            }
        }

        public IEnumerable<IIdentityUserToken> GetExternalLoginTokens(Guid userOrMemberKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserToken>().Where(x => x.Key == userOrMemberKey))
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
        public void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.Save(userOrMemberKey, logins);
                scope.Complete();
            }
        }

        public void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.Save(userOrMemberKey, tokens);
                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void DeleteUserLogins(Guid userOrMemberKey)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.DeleteUserLogins(userOrMemberKey);
                scope.Complete();
            }
        }
    }

    public class ExternalLoginService : RepositoryService, IExternalLoginService
    {
        private readonly IExternalLoginRepository _externalLoginRepository;

        public ExternalLoginService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginRepository externalLoginRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _externalLoginRepository = externalLoginRepository;
        }

        /// <inheritdoc />
        public IEnumerable<IIdentityUserLogin> GetExternalLogins(int userId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // TODO: This is temp until we update the external service to support guids for both users and members
                var asString = userId.ToString(CultureInfo.InvariantCulture);
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.UserId == asString))
                    .ToList();
            }
        }

        public IEnumerable<IIdentityUserToken> GetExternalLoginTokens(int userId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // TODO: This is temp until we update the external service to support guids for both users and members
                var asString = userId.ToString(CultureInfo.InvariantCulture);
                return _externalLoginRepository.Get(Query<IIdentityUserToken>().Where(x => x.UserId == asString))
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

        public void Save(int userId, IEnumerable<IExternalLoginToken> tokens)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _externalLoginRepository.Save(userId, tokens);
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
