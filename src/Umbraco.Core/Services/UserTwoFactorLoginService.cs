using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="Umbraco.Cms.Core.Services.IUserTwoFactorLoginService" />
internal class UserTwoFactorLoginService : TwoFactorLoginServiceBase, IUserTwoFactorLoginService
{
    private readonly IUserService _userService;

    public UserTwoFactorLoginService(
        ITwoFactorLoginService twoFactorLoginService,
        IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators,
        IUserService userService,
        ICoreScopeProvider scopeProvider)
        : base(twoFactorLoginService, twoFactorSetupGenerators, scopeProvider) =>
        _userService = userService;

    /// <inheritdoc cref="IUserTwoFactorLoginService.DisableAsync" />
    public override async Task<Attempt<TwoFactorOperationStatus>> DisableAsync(Guid userKey, string providerName)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return Attempt.Fail(TwoFactorOperationStatus.UserNotFound);
        }

        return await base.DisableAsync(userKey, providerName);
    }

    /// <inheritdoc cref="IUserTwoFactorLoginService.DisableAsync" />
    public override async Task<Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus>> GetProviderNamesAsync(Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return Attempt.FailWithStatus(TwoFactorOperationStatus.UserNotFound, Enumerable.Empty<UserTwoFactorProviderModel>());
        }

        return await base.GetProviderNamesAsync(userKey);
    }

    /// <inheritdoc cref="IUserTwoFactorLoginService.DisableAsync" />
    public override async Task<Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus>> GetSetupInfoAsync(Guid userKey, string providerName)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return Attempt.FailWithStatus<ISetupTwoFactorModel, TwoFactorOperationStatus>(TwoFactorOperationStatus.UserNotFound, new NoopSetupTwoFactorModel());
        }

        return await base.GetSetupInfoAsync(userKey, providerName);
    }

    /// <inheritdoc cref="IUserTwoFactorLoginService.DisableAsync" />
    public override async Task<Attempt<TwoFactorOperationStatus>> ValidateAndSaveAsync(string providerName, Guid userKey, string secret, string code)
    {
        IUser? user = await _userService.GetAsync(userKey);

        if (user is null)
        {
            return Attempt.Fail(TwoFactorOperationStatus.UserNotFound);
        }

        return await base.ValidateAndSaveAsync(providerName, userKey, secret, code);
    }
}
