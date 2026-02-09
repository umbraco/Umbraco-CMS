namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     The types of members to count
/// </summary>
public enum MemberCountType
{
    /// <summary>
    ///     Count all members.
    /// </summary>
    All,

    /// <summary>
    ///     Count only locked out members.
    /// </summary>
    LockedOut,

    /// <summary>
    ///     Count only approved members.
    /// </summary>
    Approved,
}
