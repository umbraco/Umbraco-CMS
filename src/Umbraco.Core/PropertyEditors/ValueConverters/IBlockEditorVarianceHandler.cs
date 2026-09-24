using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Handles value variance for the Block Editor property editor, determining how property values differ
///     based on culture and segment.
/// </summary>
/// <remarks>
///     This exposes the part of <c>BlockEditorVarianceHandler</c> that is needed across the layer boundary,
///     and grows as more of it is. Everything else on the handler is still reached through the concrete type.
/// </remarks>
public interface IBlockEditorVarianceHandler
{
    /// <summary>
    ///     Aligns block property values to their own property types' culture variance.
    /// </summary>
    /// <param name="blockPropertyValues">
    ///     The values to align. Every one must have its <see cref="BlockPropertyValue.PropertyType" /> resolved.
    /// </param>
    /// <param name="culture">
    ///     The culture to give values that need one but do not have it, or <c>null</c> for the default.
    /// </param>
    /// <returns>
    ///     The values that survive alignment. Culture-specific leftovers of a property type that no longer
    ///     varies are collapsed to one, so this can be shorter than what went in.
    /// </returns>
    /// <remarks>
    ///     Block property values carry the variation they were stored under, which is not always the variation
    ///     their property types allow: an element type's variance can change after the values were written, and
    ///     a block's values are stored under its variance intersected with whatever contains it.
    /// </remarks>
    Task<IList<BlockPropertyValue>> AlignPropertyVarianceAsync(IList<BlockPropertyValue> blockPropertyValues, string? culture);
}
