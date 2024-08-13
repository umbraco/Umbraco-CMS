using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A user specific Two factor service, that ensures the user exists before doing the job.
/// </summary>
public interface IUserTwoFactorLoginService
{
    /// <summary>
    /// Disables a specific two factor provider on a specific user.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> DisableAsync(Guid userKey, string providerName);

    /// <summary>
    /// Gets the two factor providers on a specific user.
    /// </summary>
    Task<Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus>> GetProviderNamesAsync(Guid userKey);

    /// <remarks>
    ///     The returned type can be anything depending on the setup providers. You will need to cast it to the type handled by
    ///     the provider.
    /// </remarks>
    Task<Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus>> GetSetupInfoAsync(Guid userKey, string providerName);

    /// <summary>
    /// Validates and Saves.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> ValidateAndSaveAsync(string providerName, Guid userKey, string modelSecret, string modelCode);

    /// <summary>
    /// Disables 2FA with Code.
    /// </summary>
    Task<Attempt<TwoFactorOperationStatus>> DisableByCodeAsync(string providerName, Guid userKey, string code);
}
