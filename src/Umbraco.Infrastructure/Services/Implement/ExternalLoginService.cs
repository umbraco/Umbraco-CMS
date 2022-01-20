using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    public class ExternalLoginService : RepositoryService, IExternalLoginService, IExternalLoginWithKeyService
    {
        private readonly IExternalLoginWithKeyRepository _externalLoginRepository;

        public ExternalLoginService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginWithKeyRepository externalLoginRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _externalLoginRepository = externalLoginRepository;
        }

        [Obsolete("Use ctor injecting IExternalLoginWithKeyRepository")]
        public ExternalLoginService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IExternalLoginRepository externalLoginRepository)
            : this(provider, loggerFactory, eventMessagesFactory, StaticServiceProvider.Instance.GetRequiredService<IExternalLoginWithKeyRepository>())
        {
        }

        /// <inheritdoc />
        [Obsolete("Use overload that takes a user/member key (Guid).")]
        public IEnumerable<IIdentityUserLogin> GetExternalLogins(int userId)
            => GetExternalLogins(userId.ToGuid());

        /// <inheritdoc />
        [Obsolete("Use overload that takes a user/member key (Guid).")]
        public IEnumerable<IIdentityUserToken> GetExternalLoginTokens(int userId) =>
            GetExternalLoginTokens(userId.ToGuid());

        /// <inheritdoc />
        [Obsolete("Use overload that takes a user/member key (Guid).")]
        public void Save(int userId, IEnumerable<IExternalLogin> logins)
            => Save(userId.ToGuid(), logins);

        /// <inheritdoc />
        [Obsolete("Use overload that takes a user/member key (Guid).")]
        public void Save(int userId, IEnumerable<IExternalLoginToken> tokens)
            => Save(userId.ToGuid(), tokens);

        /// <inheritdoc />
        [Obsolete("Use overload that takes a user/member key (Guid).")]
        public void DeleteUserLogins(int userId)
            => DeleteUserLogins(userId.ToGuid());

        /// <inheritdoc />
        public IEnumerable<IIdentityUserLogin> GetExternalLogins(Guid userOrMemberKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.Key == userOrMemberKey))
                    .ToList();
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
}
