// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Extensions;

public static class IntExtensions
{
    /// <summary>
    ///     Does something 'x' amount of times.
    /// </summary>
    /// <param name="n">Number of times to execute the action.</param>
    /// <param name="action">The action to execute.</param>
    public static void Times(this int n, Action<int> action)
    {
        for (var i = 0; i < n; i++)
        {
            action(i);
        }
    }

    /// <summary>
    ///     Creates a Guid based on an integer value.
    /// </summary>
    /// <param name="value">The <see cref="int" /> value to convert.</param>
    /// <returns>
    ///     The converted <see cref="Guid" />.
    /// </returns>
    public static Guid ToGuid(this int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    /// <summary>
    ///     Restores a GUID previously created from an integer value using <see cref="ToGuid" />.
    /// </summary>
    /// <param name="value">The <see cref="Guid" /> value to convert.</param>
    /// <param name="result">The converted <see cref="int" />.</param>
    /// <returns>
    ///     True if the <see cref="int" /> value could be created, otherwise false.
    /// </returns>
    /// <remarks>
    ///     This is used with Umbraco entities that only have integer references in the database (e.g. users).
    /// </remarks>
    public static bool TryParseFromGuid(Guid value, [NotNullWhen(true)] out int? result)
    {
        if (value.ToString().EndsWith("-0000-0000-0000-000000000000") is false)
        {
            // We have a proper GUID, not one converted from an integer.
            result = null;
            return false;
        }

        result = BitConverter.ToInt32(value.ToByteArray());
        return true;
    }
}
