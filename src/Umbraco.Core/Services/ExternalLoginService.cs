using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class ExternalLoginService : RepositoryService, IExternalLoginService, IExternalLoginWithKeyService
{
    private readonly IExternalLoginWithKeyRepository _externalLoginRepository;

    public ExternalLoginService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IExternalLoginWithKeyRepository externalLoginRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _externalLoginRepository = externalLoginRepository;

    [Obsolete("Use ctor injecting IExternalLoginWithKeyRepository")]
    public ExternalLoginService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IExternalLoginRepository externalLoginRepository)
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
}
