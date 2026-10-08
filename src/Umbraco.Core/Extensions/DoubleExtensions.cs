using System.Globalization;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="double" /> and <see cref="float" />.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    ///     Converts a floating-point value to a <see cref="decimal" /> using its shortest round-trippable
    ///     representation rather than its exact binary expansion.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The decimal a person would write for the value, so <c>1.65</c> becomes <c>1.65m</c>.</returns>
    /// <remarks>
    ///     The built-in conversions are exact from .NET 11, which turns <c>1.65</c> into
    ///     <c>1.6499999999999999111821580300m</c>. Use this for values that originate as human-entered
    ///     decimals, such as editor input and data type configuration, where the shortest representation
    ///     is the intended one.
    /// </remarks>
    /// <exception cref="OverflowException">The value is not finite or is outside the range of <see cref="decimal" />.</exception>
    public static decimal ToShortestDecimal(this double value)
    {
        if (double.IsFinite(value) is false)
        {
            throw new OverflowException($"Value {value} cannot be represented as a decimal.");
        }

        return decimal.Parse(value.ToString("R", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /// <inheritdoc cref="ToShortestDecimal(double)" />
    public static decimal ToShortestDecimal(this float value)
    {
        if (float.IsFinite(value) is false)
        {
            throw new OverflowException($"Value {value} cannot be represented as a decimal.");
        }

        return decimal.Parse(value.ToString("R", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
