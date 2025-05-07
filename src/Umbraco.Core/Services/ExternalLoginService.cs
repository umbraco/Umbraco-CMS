using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class ExternalLoginService : RepositoryService, IExternalLoginWithKeyService
{
    private readonly IExternalLoginWithKeyRepository _externalLoginRepository;

    public ExternalLoginService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IExternalLoginWithKeyRepository externalLoginRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _externalLoginRepository = externalLoginRepository;

    public IEnumerable<IIdentityUserLogin> Find(string loginProvider, string providerKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _externalLoginRepository.Get(Query<IIdentityUserLogin>()
                    .Where(x => x.ProviderKey == providerKey && x.LoginProvider == loginProvider))
                .ToList();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IIdentityUserLogin> GetExternalLogins(Guid userOrMemberKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _externalLoginRepository.Get(Query<IIdentityUserLogin>().Where(x => x.Key == userOrMemberKey))
                .ToList();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IIdentityUserToken> GetExternalLoginTokens(Guid userOrMemberKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _externalLoginRepository.Get(Query<IIdentityUserToken>().Where(x => x.Key == userOrMemberKey))
                .ToList();
        }
    }

    /// <inheritdoc />
    public void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _externalLoginRepository.Save(userOrMemberKey, logins);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _externalLoginRepository.Save(userOrMemberKey, tokens);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public void DeleteUserLogins(Guid userOrMemberKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _externalLoginRepository.DeleteUserLogins(userOrMemberKey);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public void DeleteUserLoginsForRemovedProviders(IEnumerable<string> currentProviderKeys)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _externalLoginRepository.DeleteUserLoginsForRemovedProviders(currentProviderKeys);
            scope.Complete();
        }
    }
}
