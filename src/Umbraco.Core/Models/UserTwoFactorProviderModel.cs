using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract]
public class UserTwoFactorProviderModel
{
    public UserTwoFactorProviderModel(string providerName, bool isEnabledOnUser)
    {
        ProviderName = providerName;
        IsEnabledOnUser = isEnabledOnUser;
    }

    [DataMember(Name = "providerName")]
    public string ProviderName { get; }

    [DataMember(Name = "isEnabledOnUser")]
    public bool IsEnabledOnUser { get; }
}
