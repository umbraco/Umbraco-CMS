namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Specifies the type of value comparison to perform in a health check.
/// </summary>
public enum ValueComparisonType
{
    /// <summary>
    ///     The actual value should equal the expected value.
    /// </summary>
    ShouldEqual,

    /// <summary>
    ///     The actual value should not equal the expected value.
    /// </summary>
    ShouldNotEqual,
}
