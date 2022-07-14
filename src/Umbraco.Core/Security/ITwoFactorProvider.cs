namespace Umbraco.Cms.Core.Security;

public interface ITwoFactorProvider
{
    string ProviderName { get; }

    Task<object> GetSetupDataAsync(Guid userOrMemberKey, string secret);

    bool ValidateTwoFactorPIN(string secret, string token);

    /// <summary>
    /// </summary>
    /// <remarks>Called to confirm the setup of two factor on the user.</remarks>
    bool ValidateTwoFactorSetup(string secret, string token);
}
