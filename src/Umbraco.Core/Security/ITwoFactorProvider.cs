namespace Umbraco.Cms.Core.Security;


/// <summary>
/// A two factor provider
/// </summary>
public interface ITwoFactorProvider
{
    /// <summary>
    /// A unique name for this provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the data needed to setup this provider. Using the marker interface ISetupTwoFactorModel.
    /// </summary>
    Task<ISetupTwoFactorModel> GetSetupDataAsync(Guid userOrMemberKey, string secret);

    /// <summary>
    /// Validates a secret on a user and a token matches doing sign-in.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    bool ValidateTwoFactorPIN(string secret, string token);

    /// <summary>
    ///     Validates a secret and a token matches doing setup, in case that is not the same as the token doing normal sign-ins.
    /// </summary>
    /// <remarks>Called to confirm the setup of two factor on the user.</remarks>
    bool ValidateTwoFactorSetup(string secret, string token);
}
