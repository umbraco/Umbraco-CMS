namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the kind or type of member.
/// </summary>
public enum MemberKind
{
    /// <summary>
    ///     A default member created through standard registration.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     A member created through the API.
    /// </summary>
    Api
}
