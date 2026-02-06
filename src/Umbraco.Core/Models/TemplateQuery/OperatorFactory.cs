namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Provides factory methods for creating <see cref="Operator" /> values from string representations.
/// </summary>
public static class OperatorFactory
{
    /// <summary>
    ///     Converts a string representation of an operator to its <see cref="Operator" /> enumeration value.
    /// </summary>
    /// <param name="stringOperator">The string representation of the operator (e.g., "=", "!=", "&lt;", "&gt;").</param>
    /// <returns>The corresponding <see cref="Operator" /> enumeration value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the string operator is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the string operator is not recognized.</exception>
    public static Operator FromString(string stringOperator)
    {
        if (stringOperator == null)
        {
            throw new ArgumentNullException(nameof(stringOperator));
        }

        switch (stringOperator)
        {
            case "=":
            case "==":
                return Operator.Equals;
            case "!=":
            case "<>":
                return Operator.NotEquals;
            case "<":
                return Operator.LessThan;
            case "<=":
                return Operator.LessThanEqualTo;
            case ">":
                return Operator.GreaterThan;
            case ">=":
                return Operator.GreaterThanEqualTo;
            default:
                throw new ArgumentException(
                    $"A operator cannot be created from the specified string '{stringOperator}'",
                    nameof(stringOperator));
        }
    }
}
