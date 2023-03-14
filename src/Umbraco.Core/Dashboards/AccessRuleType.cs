namespace Umbraco.Cms.Core.Dashboards;

/// <summary>
///     Defines dashboard access rules type.
/// </summary>
public enum AccessRuleType
{
    /// <summary>
    ///     Unknown (default value).
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Grant access to the dashboard if user belongs to the specified user group.
    /// </summary>
    Grant,

    /// <summary>
    ///     Deny access to the dashboard if user belongs to the specified user group.
    /// </summary>
    Deny,

    /// <summary>
    ///     Grant access to the dashboard if user has access to the specified section.
    /// </summary>
    GrantBySection,
}
