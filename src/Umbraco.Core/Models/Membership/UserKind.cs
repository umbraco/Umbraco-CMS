namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the kind or type of user.
/// </summary>
public enum UserKind
{
    /// <summary>
    ///     A default backoffice user.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     An API-only user that cannot log in to the backoffice.
    /// </summary>
    Api
}
