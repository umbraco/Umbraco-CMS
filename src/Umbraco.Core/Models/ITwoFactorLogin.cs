using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a two-factor authentication login configuration.
/// </summary>
public interface ITwoFactorLogin : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets the name of the two-factor authentication provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     Gets the secret key used for two-factor authentication.
    /// </summary>
    string Secret { get; }

    /// <summary>
    ///     Gets the unique identifier of the user or member associated with this login.
    /// </summary>
    Guid UserOrMemberKey { get; }
}
