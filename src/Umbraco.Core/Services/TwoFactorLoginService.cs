using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class TwoFactorLoginService : ITwoFactorLoginService2
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

    [Obsolete("Use ctor with all params - This will be removed in v11")]
    public TwoFactorLoginService(
        ITwoFactorLoginRepository twoFactorLoginRepository,
        ICoreScopeProvider scopeProvider,
        IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators,
        IOptions<IdentityOptions> identityOptions,
        IOptions<BackOfficeIdentityOptions> backOfficeIdentityOptions)
        : this(
            twoFactorLoginRepository,
            scopeProvider,
            twoFactorSetupGenerators,
            identityOptions,
            backOfficeIdentityOptions,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<TwoFactorLoginService>>())
    {
    }

    /// <inheritdoc />
    public async Task DeleteUserLoginsAsync(Guid userOrMemberKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetEnabledTwoFactorProviderNamesAsync(Guid userOrMemberKey) =>
        await GetEnabledProviderNamesAsync(userOrMemberKey);

    public async Task<bool> DisableWithCodeAsync(string providerName, Guid userOrMemberKey, string code)
    {
        var secret = await GetSecretForUserAndProviderAsync(userOrMemberKey, providerName);

        if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider? generator))
        {
            throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
        }

        var isValid = secret is not null && generator.ValidateTwoFactorPIN(secret, code);

        if (!isValid)
        {
            return false;
        }

        return await DisableAsync(userOrMemberKey, providerName);
    }

    public async Task<bool> ValidateAndSaveAsync(string providerName, Guid userOrMemberKey, string secret, string code)
    {
        try
        {
            var isValid = ValidateTwoFactorSetup(providerName, secret, code);
            if (isValid == false)
            {
                return false;
            }

            var twoFactorLogin = new TwoFactorLogin
            {
                Confirmed = true,
                Secret = secret,
                UserOrMemberKey = userOrMemberKey,
                ProviderName = providerName,
            };

            await SaveAsync(twoFactorLogin);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not log in with the provided one-time-password");
        }

        return false;
    }

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
    public async Task<object?> GetSetupInfoAsync(Guid userOrMemberKey, string providerName)
    {
        var secret = await GetSecretForUserAndProviderAsync(userOrMemberKey, providerName);

        // Dont allow to generate a new secrets if user already has one
        if (!string.IsNullOrEmpty(secret))
        {
            return default;
        }

        secret = GenerateSecret();

        if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider? generator))
        {
            throw new InvalidOperationException($"No ITwoFactorSetupGenerator found for provider: {providerName}");
        }

        return await generator.GetSetupDataAsync(userOrMemberKey, secret);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllProviderNames() => _twoFactorSetupGenerators.Keys;

    /// <inheritdoc />
    public async Task<bool> DisableAsync(Guid userOrMemberKey, string providerName)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _twoFactorLoginRepository.DeleteUserLoginsAsync(userOrMemberKey, providerName);
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
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        _twoFactorLoginRepository.Save(twoFactorLogin);

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
