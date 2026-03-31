// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

public static class ContentTypeChangeExtensions
{
    public static bool HasType(this ContentTypeChangeTypes change, ContentTypeChangeTypes type) =>
        (change & type) != ContentTypeChangeTypes.None;

    public static bool HasTypesAll(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == types;

    public static bool HasTypesAny(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) != ContentTypeChangeTypes.None;

    public static bool HasTypesNone(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == ContentTypeChangeTypes.None;

    /// <summary>
    ///     Determines whether the change has structural change impact.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <returns><c>true</c> if the change has structural impact; otherwise, <c>false</c>.</returns>
    public static bool IsStructuralChange(this ContentTypeChangeTypes change) =>
        change.HasType(ContentTypeChangeTypes.RefreshMain);

    /// <summary>
    ///     Determines whether the change has non-structural change impact.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <returns><c>true</c> if the change has non-structural impact; otherwise, <c>false</c>.</returns>
    public static bool IsNonStructuralChange(this ContentTypeChangeTypes change) =>
        change.HasType(ContentTypeChangeTypes.RefreshOther) && !change.HasType(ContentTypeChangeTypes.RefreshMain);
}
