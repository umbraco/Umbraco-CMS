// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for content type changes.
/// </summary>
public static class ContentTypeChangeExtensions
{
    /// <summary>
    ///     Determines whether the change includes the specified type.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns><c>true</c> if the change includes the type; otherwise, <c>false</c>.</returns>
    public static bool HasType(this ContentTypeChangeTypes change, ContentTypeChangeTypes type) =>
        (change & type) != ContentTypeChangeTypes.None;

    /// <summary>
    ///     Determines whether the change includes all of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes all types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesAll(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == types;

    /// <summary>
    ///     Determines whether the change includes any of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes any of the types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesAny(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) != ContentTypeChangeTypes.None;

    /// <summary>
    ///     Determines whether the change includes none of the specified types.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <param name="types">The types to look for.</param>
    /// <returns><c>true</c> if the change includes none of the types; otherwise, <c>false</c>.</returns>
    public static bool HasTypesNone(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == ContentTypeChangeTypes.None;
}
