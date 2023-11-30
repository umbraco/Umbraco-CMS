using System.Reflection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the types of default sorting options.
/// </summary>
/// <remarks>
///     <para>
///         These types are used to determine the storage type, but also for
///         validation. Therefore, they are more detailed than the storage types.
///     </para>
/// </remarks>
public static class SortByTypes
{
    /// <summary>
    ///     Name.
    /// </summary>
    public const string Name = "name";

    /// <summary>
    ///     Update date.
    /// </summary>
    public const string UpdateDate = "updateDate";

    // the auto, static, set of valid values
    private static readonly HashSet<string?> Values
        = new(typeof(ValueTypes)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.IsLiteral && !x.IsInitOnly)
            .Select(x => (string?)x.GetRawConstantValue()));

    /// <summary>
    ///     Determines whether a string value is a valid SortByTypes value.
    /// </summary>
    public static bool IsValue(string s)
        => Values.Contains(s);

    /// <summary>
    ///     Gets the <see cref="ValueStorageType" /> value corresponding to a SortByTypes value.
    /// </summary>
    public static ValueStorageType ToStorageType(string valueType)
    {
        switch (valueType.ToUpperInvariant())
        {
            case Name:
            case UpdateDate:
                return ValueStorageType.Nvarchar;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(valueType),
                    $"Value \"{valueType}\" is not a valid SortByType.");
        }
    }
}
