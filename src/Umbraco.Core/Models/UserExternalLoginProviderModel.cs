namespace Umbraco.Cms.Core.Models;

public class UserExternalLoginProviderModel
{
    public UserExternalLoginProviderModel(string providerSchemeName, bool isLinkedOnUser, bool hasManualLinkingEnabled)
    {
        ProviderSchemeName = providerSchemeName;
        IsLinkedOnUser = isLinkedOnUser;
        HasManualLinkingEnabled = hasManualLinkingEnabled;
    }

    public string ProviderSchemeName { get; }

    public bool IsLinkedOnUser { get; }

    public bool HasManualLinkingEnabled { get; }
}
