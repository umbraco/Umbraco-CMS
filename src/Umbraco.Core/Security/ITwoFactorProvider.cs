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
    /// Gets the data needed to setup this provider. Using the marker interface <see cref="ISetupTwoFactorModel" />.
    /// </summary>
    Task<ISetupTwoFactorModel> GetSetupDataAsync(Guid userOrMemberKey, string secret);

    /// <summary>
    /// Validates the 2FA login token for the user identified by the supplied secret.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    bool ValidateTwoFactorPIN(string secret, string token);

    /// <summary>
    /// Validates the 2FA setup token for the user identified by the supplied secret.
    /// </summary>
    /// <remarks>Called to confirm the setup of two factor on the user.</remarks>
    bool ValidateTwoFactorSetup(string secret, string token);
}
