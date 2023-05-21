using System.Reflection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the types of the edited values.
/// </summary>
/// <remarks>
///     <para>
///         These types are used to determine the storage type, but also for
///         validation. Therefore, they are more detailed than the storage types.
///     </para>
/// </remarks>
public static class ValueTypes
{
    /// <summary>
    ///     Date value.
    /// </summary>
    public const string Date = "DATE"; // Date

    /// <summary>
    ///     DateTime value.
    /// </summary>
    public const string DateTime = "DATETIME"; // Date

    /// <summary>
    ///     Decimal value.
    /// </summary>
    public const string Decimal = "DECIMAL"; // Decimal

    /// <summary>
    ///     Integer value.
    /// </summary>
    public const string Integer = "INT"; // Integer

    /// <summary>
    ///     Integer value.
    /// </summary>
    public const string Bigint = "BIGINT"; // String

    /// <summary>
    ///     Json value.
    /// </summary>
    public const string Json = "JSON"; // NText

    /// <summary>
    ///     Text value (maps to text database type).
    /// </summary>
    public const string Text = "TEXT"; // NText

    /// <summary>
    ///     Time value.
    /// </summary>
    public const string Time = "TIME"; // Date

    /// <summary>
    ///     Text value (maps to varchar database type).
    /// </summary>
    public const string String = "STRING"; // NVarchar

    /// <summary>
    ///     Xml value.
    /// </summary>
    public const string Xml = "XML"; // NText

    // the auto, static, set of valid values
    private static readonly HashSet<string?> Values
        = new(typeof(ValueTypes)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.IsLiteral && !x.IsInitOnly)
            .Select(x => (string?)x.GetRawConstantValue()));

    /// <summary>
    ///     Determines whether a string value is a valid ValueTypes value.
    /// </summary>
    public static bool IsValue(string s)
        => Values.Contains(s);

    /// <summary>
    ///     Gets the <see cref="ValueStorageType" /> value corresponding to a ValueTypes value.
    /// </summary>
    public static ValueStorageType ToStorageType(string valueType)
    {
        switch (valueType.ToUpperInvariant())
        {
            case Integer:
                return ValueStorageType.Integer;

            case Decimal:
                return ValueStorageType.Decimal;

            case String:
            case Bigint:
                return ValueStorageType.Nvarchar;

            case Text:
            case Json:
            case Xml:
                return ValueStorageType.Ntext;

            case DateTime:
            case Date:
            case Time:
                return ValueStorageType.Date;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(valueType),
                    $"Value \"{valueType}\" is not a valid ValueTypes.");
        }
    }
}
