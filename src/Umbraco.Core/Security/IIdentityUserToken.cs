using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     An external login provider token
/// </summary>
public interface IIdentityUserToken : IEntity
{
    /// <summary>
    ///     Gets or sets user Id for the user who owns this token
    /// </summary>
    string? UserId { get; set; }

    /// <summary>
    ///     Gets or sets the login provider for the login (i.e. Facebook, Google)
    /// </summary>
    string LoginProvider { get; set; }

    /// <summary>
    ///     Gets or sets the token name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets or set the token value
    /// </summary>
    string Value { get; set; }
}
