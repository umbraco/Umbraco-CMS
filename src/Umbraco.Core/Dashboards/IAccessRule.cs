namespace Umbraco.Cms.Core.Dashboards;

/// <summary>
///     Represents an access rule.
/// </summary>
public interface IAccessRule
{
    /// <summary>
    ///     Gets or sets the rule type.
    /// </summary>
    AccessRuleType Type { get; set; }

    /// <summary>
    ///     Gets or sets the value for the rule.
    /// </summary>
    string? Value { get; set; }
}
