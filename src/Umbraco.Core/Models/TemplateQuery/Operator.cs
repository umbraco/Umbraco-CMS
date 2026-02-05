namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents the comparison operators available for template query conditions.
/// </summary>
public enum Operator
{
    /// <summary>
    ///     Checks if the value equals the constraint.
    /// </summary>
    Equals = 1,

    /// <summary>
    ///     Checks if the value does not equal the constraint.
    /// </summary>
    NotEquals = 2,

    /// <summary>
    ///     Checks if the string value contains the constraint.
    /// </summary>
    Contains = 3,

    /// <summary>
    ///     Checks if the string value does not contain the constraint.
    /// </summary>
    NotContains = 4,

    /// <summary>
    ///     Checks if the value is less than the constraint.
    /// </summary>
    LessThan = 5,

    /// <summary>
    ///     Checks if the value is less than or equal to the constraint.
    /// </summary>
    LessThanEqualTo = 6,

    /// <summary>
    ///     Checks if the value is greater than the constraint.
    /// </summary>
    GreaterThan = 7,

    /// <summary>
    ///     Checks if the value is greater than or equal to the constraint.
    /// </summary>
    GreaterThanEqualTo = 8,
}
