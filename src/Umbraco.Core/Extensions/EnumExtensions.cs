// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="Enum" />.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///     Determines whether all the flags/bits are set within the enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <param name="flags">The flags.</param>
    /// <returns>
    ///     <c>true</c> if all the flags/bits are set within the enum value; otherwise, <c>false</c>.
    /// </returns>
    [Obsolete("Use Enum.HasFlag() or bitwise operations (if performance is important) instead.")]
    public static bool HasFlagAll<T>(this T value, T flags)
        where T : Enum =>
        value.HasFlag(flags);

    /// <summary>
    ///     Determines whether any of the flags/bits are set within the enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="flags">The flags.</param>
    /// <returns>
    ///     <c>true</c> if any of the flags/bits are set within the enum value; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasFlagAny<T>(this T value, T flags)
        where T : Enum
    {
        var v = Convert.ToUInt64(value);
        var f = Convert.ToUInt64(flags);

        return (v & f) > 0;
    }
}
