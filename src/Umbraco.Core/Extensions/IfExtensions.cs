// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for 'If' checking like checking If something is null or not null
/// </summary>
public static class IfExtensions
{
    /// <summary>The if not null.</summary>
    /// <param name="item">The item.</param>
    /// <param name="action">The action.</param>
    /// <typeparam name="TItem">The type</typeparam>
    public static void IfNotNull<TItem>(this TItem item, Action<TItem> action)
        where TItem : class
    {
        if (item != null)
        {
            action(item);
        }
    }

    /// <summary>The if true.</summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="action">The action.</param>
    public static void IfTrue(this bool predicate, Action action)
    {
        if (predicate)
        {
            action();
        }
    }

    /// <summary>
    ///     Checks if the item is not null, and if so returns an action on that item, or a default value
    /// </summary>
    /// <typeparam name="TResult">the result type</typeparam>
    /// <typeparam name="TItem">The type</typeparam>
    /// <param name="item">The item.</param>
    /// <param name="action">The action.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static TResult? IfNotNull<TResult, TItem>(this TItem? item, Func<TItem, TResult> action, TResult? defaultValue = default)
        where TItem : class
        => item != null ? action(item) : defaultValue;

    /// <summary>
    ///     Checks if the value is null, if it is it returns the value specified, otherwise returns the non-null value
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="item"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static TItem IfNull<TItem>(this TItem? item, Func<TItem, TItem> action)
        where TItem : class
        => item ?? action(item!);
}
