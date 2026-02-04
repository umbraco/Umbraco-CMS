namespace Umbraco.Cms.Core.Persistence.Querying;

/// <summary>
///     Determines how to match a number or date value.
/// </summary>
public enum ValuePropertyMatchType
{
    /// <summary>
    ///     Matches the exact value.
    /// </summary>
    Exact,

    /// <summary>
    ///     Matches values greater than the specified value.
    /// </summary>
    GreaterThan,

    /// <summary>
    ///     Matches values less than the specified value.
    /// </summary>
    LessThan,

    /// <summary>
    ///     Matches values greater than or equal to the specified value.
    /// </summary>
    GreaterThanOrEqualTo,

    /// <summary>
    ///     Matches values less than or equal to the specified value.
    /// </summary>
    LessThanOrEqualTo,
}
