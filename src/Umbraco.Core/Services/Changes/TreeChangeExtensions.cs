// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for tree changes.
/// </summary>
public static class TreeChangeExtensions
{
    /// <summary>
    ///     Converts a collection of tree changes to event arguments.
    /// </summary>
    /// <typeparam name="TItem">The type of the item that changed.</typeparam>
    /// <param name="changes">The changes to convert.</param>
    /// <returns>Event arguments containing the changes.</returns>
    public static TreeChange<TItem>.EventArgs ToEventArgs<TItem>(this IEnumerable<TreeChange<TItem>> changes) =>
        new TreeChange<TItem>.EventArgs(changes);

    /// <summary>
    ///     Determines whether the change includes the specified type.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns><c>true</c> if the change includes the type; otherwise, <c>false</c>.</returns>
    public static bool HasType(this TreeChangeTypes change, TreeChangeTypes type) =>
        (change & type) != TreeChangeTypes.None;

    /// <summary>
    ///     Determines whether the change includes all of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes all types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesAll(this TreeChangeTypes change, TreeChangeTypes types) => (change & types) == types;

    /// <summary>
    ///     Determines whether the change includes any of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes any of the types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesAny(this TreeChangeTypes change, TreeChangeTypes types) =>
        (change & types) != TreeChangeTypes.None;

    /// <summary>
    ///     Determines whether the change includes none of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes none of the types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesNone(this TreeChangeTypes change, TreeChangeTypes types) =>
        (change & types) == TreeChangeTypes.None;
}
