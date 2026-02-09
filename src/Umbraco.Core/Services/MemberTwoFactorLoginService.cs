using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="Umbraco.Cms.Core.Services.IMemberTwoFactorLoginService" />
internal sealed class MemberTwoFactorLoginService : TwoFactorLoginServiceBase, IMemberTwoFactorLoginService
{
    private readonly IMemberService _memberService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTwoFactorLoginService" /> class.
    /// </summary>
    /// <param name="twoFactorLoginService">The two-factor login service.</param>
    /// <param name="twoFactorSetupGenerators">The collection of two-factor providers.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="scopeProvider">The core scope provider.</param>
    public MemberTwoFactorLoginService(
        ITwoFactorLoginService twoFactorLoginService,
        IEnumerable<ITwoFactorProvider> twoFactorSetupGenerators,
        IMemberService memberService,
        ICoreScopeProvider scopeProvider)
        : base(twoFactorLoginService, twoFactorSetupGenerators, scopeProvider) =>
        _memberService = memberService;

    /// <inheritdoc cref="IMemberTwoFactorLoginService.DisableAsync" />
    public override async Task<Attempt<TwoFactorOperationStatus>> DisableAsync(Guid memberKey, string providerName)
    {
        IMember? member = _memberService.GetByKey(memberKey);

        if (member is null)
        {
            return Attempt.Fail(TwoFactorOperationStatus.UserNotFound);
        }

        return await base.DisableAsync(memberKey, providerName);
    }

    /// <inheritdoc cref="IMemberTwoFactorLoginService.GetProviderNamesAsync" />
    public override async Task<Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus>> GetProviderNamesAsync(Guid memberKey)
    {
        IMember? member = _memberService.GetByKey(memberKey);

        if (member is null)
        {
            return Attempt.FailWithStatus(TwoFactorOperationStatus.UserNotFound, Enumerable.Empty<UserTwoFactorProviderModel>());
        }

        return await base.GetProviderNamesAsync(memberKey);
    }

    /// <inheritdoc cref="IMemberTwoFactorLoginService.GetSetupInfoAsync" />
    public override async Task<Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus>> GetSetupInfoAsync(Guid memberKey, string providerName)
    {
        IMember? member = _memberService.GetByKey(memberKey);

        if (member is null)
        {
            return Attempt.FailWithStatus<ISetupTwoFactorModel, TwoFactorOperationStatus>(TwoFactorOperationStatus.UserNotFound, new NoopSetupTwoFactorModel());
        }

        return await base.GetSetupInfoAsync(memberKey, providerName);
    }

    /// <inheritdoc cref="IMemberTwoFactorLoginService.ValidateAndSaveAsync" />
    public override async Task<Attempt<TwoFactorOperationStatus>> ValidateAndSaveAsync(string providerName, Guid memberKey, string secret, string code)
    {
        IMember? member = _memberService.GetByKey(memberKey);

        if (member is null)
        {
            return Attempt.Fail(TwoFactorOperationStatus.UserNotFound);
        }

        return await base.ValidateAndSaveAsync(providerName, memberKey, secret, code);
    }
}
