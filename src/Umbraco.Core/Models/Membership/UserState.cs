namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     The state of a user
/// </summary>
public enum UserState
{
    /// <summary>
    ///     Represents all user states (used for filtering).
    /// </summary>
    All = -1,

    /// <summary>
    ///     The user is active and can log in.
    /// </summary>
    Active = 0,

    /// <summary>
    ///     The user is disabled and cannot log in.
    /// </summary>
    Disabled = 1,

    /// <summary>
    ///     The user is locked out due to too many failed login attempts.
    /// </summary>
    LockedOut = 2,

    /// <summary>
    ///     The user has been invited but has not yet accepted.
    /// </summary>
    Invited = 3,

    /// <summary>
    ///     The user has never logged in.
    /// </summary>
    Inactive = 4,
}
