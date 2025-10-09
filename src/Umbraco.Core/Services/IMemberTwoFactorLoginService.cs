using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A member specific Two factor service, that ensures the member exists before doing the job.
/// </summary>
public interface IMemberTwoFactorLoginService
{
    /// <summary>
    /// Disables a specific two factor provider on a specific member.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> DisableAsync(Guid memberKey, string providerName);

    /// <summary>
    /// Gets the two factor providers on a specific member.
    /// </summary>
    Task<Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus>> GetProviderNamesAsync(Guid memberKey);

    /// <remarks>
    ///     The returned type can be anything depending on the setup providers. You will need to cast it to the type handled by
    ///     the provider.
    /// </remarks>
    Task<Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus>> GetSetupInfoAsync(Guid memberKey, string providerName);

    /// <summary>
    /// Validates and Saves.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> ValidateAndSaveAsync(string providerName, Guid memberKey, string modelSecret, string modelCode);

    /// <summary>
    /// Disables 2FA with Code.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> DisableByCodeAsync(string providerName, Guid memberKey, string code);
}
