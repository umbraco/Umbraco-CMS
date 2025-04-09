// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

public static class IntExtensions
{
    /// <summary>
    ///     Does something 'x' amount of times
    /// </summary>
    /// <param name="n"></param>
    /// <param name="action"></param>
    public static void Times(this int n, Action<int> action)
    {
        for (var i = 0; i < n; i++)
        {
            action(i);
        }
    }

    /// <summary>
    ///     Creates a Guid based on an integer value
    /// </summary>
    /// <param name="value"><see cref="int" /> value to convert</param>
    /// <returns>
    ///     <see cref="Guid" />
    /// </returns>
    public static Guid ToGuid(this int value)
    {
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes);
        return new Guid(bytes);
    }
}
