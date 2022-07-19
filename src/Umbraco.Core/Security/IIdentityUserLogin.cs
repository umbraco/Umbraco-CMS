using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     An external login provider linked to a user
/// </summary>
/// <typeparam name="TKey">The PK type for the user</typeparam>
public interface IIdentityUserLogin : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the login provider for the login (i.e. Facebook, Google)
    /// </summary>
    string LoginProvider { get; set; }

    /// <summary>
    ///     Gets or sets key representing the login for the provider
    /// </summary>
    string ProviderKey { get; set; }

    /// <summary>
    ///     Gets or sets user or member key (Guid) for the user/member who owns this login
    /// </summary>
    string UserId { get; set; } // TODO: This should be able to be used by both users and members

    /// <summary>
    ///     Gets or sets any arbitrary data for the user and external provider
    /// </summary>
    string? UserData { get; set; }
}
