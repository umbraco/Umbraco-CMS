namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     The state of a user
/// </summary>
public enum UserState
{
    All = -1,
    Active = 0,
    Disabled = 1,
    LockedOut = 2,
    Invited = 3,
    Inactive = 4,
}
