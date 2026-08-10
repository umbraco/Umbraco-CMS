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

    /// <summary>
    ///     Determines whether the change requires the raw database cache (<c>cmsContentNu</c>) to be rebuilt.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <returns>
    ///     <c>true</c> for a structural change unless it is flagged <see cref="ContentTypeChangeTypes.RawDataUnaffected"/>
    ///     (e.g. a property removal), in which case the stored blob stays valid and only the converted cache needs clearing.
    /// </returns>
    public static bool RequiresRawDataRebuild(this ContentTypeChangeTypes change) =>
        change.IsStructuralChange() && !change.HasType(ContentTypeChangeTypes.RawDataUnaffected);

    /// <summary>
    ///     Determines whether the change only requires the converted (in-memory) content cache to be cleared,
    ///     leaving the stored database cache (<c>cmsContentNu</c>) and HybridCache entries valid.
    /// </summary>
    /// <param name="change">The change to check.</param>
    /// <returns>
    ///     <c>true</c> for a non-structural change, or a structural change flagged
    ///     <see cref="ContentTypeChangeTypes.RawDataUnaffected"/> (e.g. a property removal).
    /// </returns>
    public static bool RequiresConvertedCacheClearOnly(this ContentTypeChangeTypes change) =>
        change.IsNonStructuralChange() || (change.IsStructuralChange() && change.HasType(ContentTypeChangeTypes.RawDataUnaffected));
}
