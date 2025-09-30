using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class TwoFactorLoginService : ITwoFactorLoginService
{
    private readonly IOptions<BackOfficeIdentityOptions> _backOfficeIdentityOptions;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly ILogger<TwoFactorLoginService> _logger;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITwoFactorLoginRepository _twoFactorLoginRepository;
    private readonly IDictionary<string, ITwoFactorProvider> _twoFactorSetupGenerators;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TwoFactorLoginService" /> class.
    /// </summary>
    public TwoFactorLoginService(
        ITwoFactorLoginRepository twoFactorLoginRepository,
        ICoreScopeProvider scopeProvider,
        IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators,
        IOptions<IdentityOptions> identityOptions,
        IOptions<BackOfficeIdentityOptions> backOfficeIdentityOptions,
        ILogger<TwoFactorLoginService> logger)
    {
        _twoFactorLoginRepository = twoFactorLoginRepository;
        _scopeProvider = scopeProvider;
        _identityOptions = identityOptions;
        _backOfficeIdentityOptions = backOfficeIdentityOptions;
        _logger = logger;
        _twoFactorSetupGenerators = twoFactorSetupGenerators.ToDictionary(x => x.ProviderName);
    }

    /// <inheritdoc />
    public async Task DeleteUserLoginsAsync(Guid userOrMemberKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey);

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetEnabledTwoFactorProviderNamesAsync(Guid userOrMemberKey) =>
        await GetEnabledProviderNamesAsync(userOrMemberKey);

    /// <inheritdoc />
    public async Task<bool> IsTwoFactorEnabledAsync(Guid userOrMemberKey) =>
        (await GetEnabledProviderNamesAsync(userOrMemberKey)).Any();

    /// <inheritdoc />
    public async Task<string?> GetSecretForUserAndProviderAsync(Guid userOrMemberKey, string providerName)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return (await _twoFactorLoginRepository.GetByUserOrMemberKeyAsync(userOrMemberKey))
            .FirstOrDefault(x => x.ProviderName == providerName)?.Secret;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllProviderNames() => _twoFactorSetupGenerators.Keys;

    /// <inheritdoc />
    public async Task<bool> DisableAsync(Guid userOrMemberKey, string providerName)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var result = await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey, providerName);

        scope.Complete();

        return result;
    }

    /// <inheritdoc />
    public bool ValidateTwoFactorSetup(string providerName, string secret, string code)
    {
        if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider? generator))
        {
            throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
        }

        return generator.ValidateTwoFactorSetup(secret, code);
    }

    /// <inheritdoc />
    public Task SaveAsync(TwoFactorLogin twoFactorLogin)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _twoFactorLoginRepository.Save(twoFactorLogin);

        scope.Complete();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Generates a new random unique secret.
    /// </summary>
    /// <returns>The random secret</returns>
    protected virtual string GenerateSecret() => Guid.NewGuid().ToString();

    private async Task<IEnumerable<string>> GetEnabledProviderNamesAsync(Guid userOrMemberKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        var providersOnUser = (await _twoFactorLoginRepository.GetByUserOrMemberKeyAsync(userOrMemberKey))
            .Select(x => x.ProviderName).ToArray();

        return providersOnUser.Where(IsKnownProviderName);
    }

    /// <summary>
    ///     The provider needs to be registered as either a member provider or backoffice provider to show up.
    /// </summary>
    private bool IsKnownProviderName(string? providerName)
    {
        if (providerName is null)
        {
            return false;
        }

        if (_identityOptions.Value.Tokens.ProviderMap.ContainsKey(providerName))
        {
            return true;
        }

        if (_backOfficeIdentityOptions.Value.Tokens.ProviderMap.ContainsKey(providerName))
        {
            return true;
        }

        return false;
    }
}
