namespace Umbraco.Cms.Core.Persistence.Querying;

/// <summary>
///     Determine how to match a number or data value
/// </summary>
public enum ValuePropertyMatchType
{
    Exact,
    GreaterThan,
    LessThan,
    GreaterThanOrEqualTo,
    LessThanOrEqualTo,
}
