using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for setting up members or users to use 2FA.
/// </summary>
internal abstract class TwoFactorLoginServiceBase
{
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDictionary<string, ITwoFactorProvider> _twoFactorSetupGenerators;

    protected TwoFactorLoginServiceBase(ITwoFactorLoginService twoFactorLoginService, IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators, ICoreScopeProvider scopeProvider)
    {
        _twoFactorLoginService = twoFactorLoginService;
        _scopeProvider = scopeProvider;
        _twoFactorSetupGenerators = twoFactorSetupGenerators.ToDictionary(x => x.ProviderName);
    }

    public virtual async Task<Attempt<TwoFactorOperationStatus>> DisableAsync(Guid userKey, string providerName)
    {
        var result = await _twoFactorLoginService.DisableAsync(userKey, providerName);

        return result
            ? Attempt.Succeed(TwoFactorOperationStatus.Success)
            : Attempt.Fail(TwoFactorOperationStatus.ProviderNameNotFound);
    }

    /// <summary>
    /// Gets the two factor providers on a specific user.
    /// </summary>
    public virtual async Task<Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus>> GetProviderNamesAsync(Guid userKey)
    {
        IEnumerable<string> allProviders = _twoFactorLoginService.GetAllProviderNames();
        var userProviders = (await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(userKey)).ToHashSet();

        IEnumerable<UserTwoFactorProviderModel> result = allProviders.Select(x => new UserTwoFactorProviderModel(x, userProviders.Contains(x)));
        return Attempt.SucceedWithStatus(TwoFactorOperationStatus.Success, result);
    }

    /// <summary>
    ///     Generates a new random unique secret.
    /// </summary>
    /// <returns>The random secret</returns>
    protected virtual string GenerateSecret() => Guid.NewGuid().ToString();

    public virtual async Task<Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus>> GetSetupInfoAsync(Guid userOrMemberKey, string providerName)
    {
        var secret = await _twoFactorLoginService.GetSecretForUserAndProviderAsync(userOrMemberKey, providerName);

        // Dont allow to generate a new secrets if user already has one
        if (!string.IsNullOrEmpty(secret))
        {
            return Attempt.FailWithStatus<ISetupTwoFactorModel, TwoFactorOperationStatus>(TwoFactorOperationStatus.ProviderAlreadySetup, new NoopSetupTwoFactorModel());
        }

        secret = GenerateSecret();

        if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider? generator))
        {
            return Attempt.FailWithStatus<ISetupTwoFactorModel, TwoFactorOperationStatus>(TwoFactorOperationStatus.ProviderNameNotFound, new NoopSetupTwoFactorModel());
        }

        ISetupTwoFactorModel result= await generator.GetSetupDataAsync(userOrMemberKey, secret);
        return Attempt.SucceedWithStatus(TwoFactorOperationStatus.Success, result);
    }

    public virtual async Task<Attempt<TwoFactorOperationStatus>> ValidateAndSaveAsync(
        string providerName,
        Guid userOrMemberKey,
        string secret,
        string code)
    {
        using var scope = _scopeProvider.CreateCoreScope();

        if ((await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(userOrMemberKey)).Contains(providerName))
        {
            return Attempt.Fail(TwoFactorOperationStatus.ProviderAlreadySetup);
        }

        bool valid;
        try
        {
            valid = _twoFactorLoginService.ValidateTwoFactorSetup(providerName, secret, code);
        }
        catch (InvalidOperationException)
        {
            return Attempt.Fail(TwoFactorOperationStatus.ProviderNameNotFound);
        }

        if (valid is false)
        {
            return Attempt.Fail(TwoFactorOperationStatus.InvalidCode);
        }

        var twoFactorLogin = new TwoFactorLogin
        {
            Confirmed = true,
            Secret = secret,
            UserOrMemberKey = userOrMemberKey,
            ProviderName = providerName,
        };


        await _twoFactorLoginService.SaveAsync(twoFactorLogin);

        scope.Complete();
        return Attempt.Succeed(TwoFactorOperationStatus.Success);
    }

    /// <summary>
    /// Disables 2FA with Code.
    /// </summary>
    public async Task<Attempt<TwoFactorOperationStatus>> DisableByCodeAsync(string providerName, Guid userOrMemberKey, string code)
    {
        var secret = await _twoFactorLoginService.GetSecretForUserAndProviderAsync(userOrMemberKey, providerName);

        if (!_twoFactorSetupGenerators.TryGetValue(providerName, out ITwoFactorProvider? generator))
        {
            return Attempt.Fail(TwoFactorOperationStatus.ProviderNameNotFound);
        }

        var isValid = secret is not null && generator.ValidateTwoFactorPIN(secret, code);

        if (!isValid)
        {
            return Attempt.Fail(TwoFactorOperationStatus.InvalidCode);
        }

        var success = await _twoFactorLoginService.DisableAsync(userOrMemberKey, providerName);

        return success
            ? Attempt.Succeed(TwoFactorOperationStatus.Success)
            : Attempt.Fail(TwoFactorOperationStatus.InvalidCode);
    }
}
