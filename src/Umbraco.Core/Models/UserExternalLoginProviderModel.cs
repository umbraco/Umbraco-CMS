namespace Umbraco.Cms.Core.Models;

public class UserExternalLoginProviderModel
{
    public UserExternalLoginProviderModel(string providerSchemeName, bool isLinkedOnUser, bool hasManualLinkingEnabled, string? providerKey)
    {
        ProviderSchemeName = providerSchemeName;
        IsLinkedOnUser = isLinkedOnUser;
        HasManualLinkingEnabled = hasManualLinkingEnabled;
        ProviderKey = providerKey;
    }

    public string ProviderSchemeName { get; }

    public string? ProviderKey { get; set; }

    public bool IsLinkedOnUser { get; }

    public bool HasManualLinkingEnabled { get; }
}
