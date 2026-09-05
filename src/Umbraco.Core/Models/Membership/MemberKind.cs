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
    Api,

    /// <summary>
    ///     An external-only member backed by the lightweight umbracoExternalMember table,
    ///     not the content system. Authenticated via an external provider.
    /// </summary>
    ExternalOnly
}
