using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a two-factor authentication login configuration for a user or member.
/// </summary>
public class TwoFactorLogin : EntityBase, ITwoFactorLogin
{
    /// <summary>
    ///     Gets or sets a value indicating whether the two-factor authentication has been confirmed/activated.
    /// </summary>
    public bool Confirmed { get; set; }

    /// <summary>
    ///     Gets or sets the name of the two-factor authentication provider.
    /// </summary>
    public string ProviderName { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the secret key used for two-factor authentication.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the unique key of the user or member associated with this two-factor login.
    /// </summary>
    public Guid UserOrMemberKey { get; set; }
}
