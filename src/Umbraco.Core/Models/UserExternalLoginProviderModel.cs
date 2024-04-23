using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

public class UserExternalLoginProviderModel
{
    public UserExternalLoginProviderModel(string providerSchemaName, bool isLinkedOnUser, bool hasManualLinkingEnabled)
    {
        ProviderSchemaName = providerSchemaName;
        IsLinkedOnUser = isLinkedOnUser;
        HasManualLinkingEnabled = hasManualLinkingEnabled;
    }

    public string ProviderSchemaName { get; }

    public bool IsLinkedOnUser { get; }

    public bool HasManualLinkingEnabled { get; }
}
