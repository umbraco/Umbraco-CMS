using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing external login providers and tokens for users and members.
/// </summary>
/// <remarks>
///     This service handles OAuth/OpenID Connect external login associations, allowing users
///     to authenticate via external providers like Google, Microsoft, Facebook, etc.
/// </remarks>
public class ExternalLoginService : RepositoryService, IExternalLoginWithKeyService
{
    private readonly IExternalLoginWithKeyRepository _externalLoginRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalLoginService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="externalLoginRepository">The repository for external login data access.</param>
    /// <param name="userRepository">The repository for user data access.</param>
    public ExternalLoginService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IExternalLoginWithKeyRepository externalLoginRepository,
        IUserRepository userRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _externalLoginRepository = externalLoginRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    ///     Finds external logins matching the specified login provider and provider key.
    /// </summary>
    /// <param name="loginProvider">The name of the external login provider (e.g., "Google", "Microsoft").</param>
    /// <param name="providerKey">The unique key assigned by the external provider for the user.</param>
    /// <returns>A collection of matching external login records.</returns>
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
    public void PurgeLoginsForRemovedProviders(IEnumerable<string> currentLoginProviders)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _userRepository.InvalidateSessionsForRemovedProviders(currentLoginProviders);
            _externalLoginRepository.DeleteUserLoginsForRemovedProviders(currentLoginProviders);
            scope.Complete();
        }
    }
}
