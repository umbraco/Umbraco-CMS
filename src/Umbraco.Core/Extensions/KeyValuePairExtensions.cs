// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="KeyValuePair{TKey,TValue}" /> struct.
/// </summary>
public static class KeyValuePairExtensions
{
    /// <summary>
    ///     Implements key/value pair deconstruction.
    /// </summary>
    /// <remarks>Allows for <c>foreach ((var k, var v) in ...)</c>.</remarks>
    [Obsolete("Please replace uses of this extension method with native language features. This method will be removed in Umbraco 17.")]
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
    {
        key = kvp.Key;
        value = kvp.Value;
    }
}
