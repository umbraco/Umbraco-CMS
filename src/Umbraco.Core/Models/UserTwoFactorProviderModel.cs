using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents information about a two-factor authentication provider and its status for a user.
/// </summary>
[DataContract]
public class UserTwoFactorProviderModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserTwoFactorProviderModel" /> class.
    /// </summary>
    /// <param name="providerName">The name of the two-factor authentication provider.</param>
    /// <param name="isEnabledOnUser">Indicates whether this provider is enabled for the user.</param>
    public UserTwoFactorProviderModel(string providerName, bool isEnabledOnUser)
    {
        ProviderName = providerName;
        IsEnabledOnUser = isEnabledOnUser;
    }

    /// <summary>
    ///     Gets the name of the two-factor authentication provider.
    /// </summary>
    [DataMember(Name = "providerName")]
    public string ProviderName { get; }

    /// <summary>
    ///     Gets a value indicating whether this two-factor provider is enabled for the user.
    /// </summary>
    [DataMember(Name = "isEnabledOnUser")]
    public bool IsEnabledOnUser { get; }
}
