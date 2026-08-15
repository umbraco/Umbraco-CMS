using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Provider.Examine.Helpers;

/// <summary>
/// Builds physical Lucene field names from a logical field name, value type, and variation (culture/segment) context.
/// </summary>
public static class FieldNameHelper
{
    /// <summary>
    /// Builds the physical Lucene field name for an <see cref="IndexField"/>.
    /// </summary>
    /// <param name="field">The index field to build the physical field name for.</param>
    /// <param name="fieldValues">The value-type suffix to use (see <see cref="Umbraco.Cms.Search.Provider.Examine.Constants.FieldValues"/>).</param>
    /// <returns>The physical Lucene field name.</returns>
    public static string FieldName(IndexField field, string fieldValues)
        => FieldName(field.FieldName, fieldValues, field.Segment);

    /// <summary>
    /// Builds the physical Lucene field name for an invariant/culture-only (non-segmented) field.
    /// </summary>
    /// <param name="fieldName">The logical field name.</param>
    /// <param name="fieldValues">The value-type suffix to use (see <see cref="Umbraco.Cms.Search.Provider.Examine.Constants.FieldValues"/>).</param>
    /// <returns>The physical Lucene field name.</returns>
    public static string FieldName(string fieldName, string fieldValues)
        => FieldName(fieldName, fieldValues, null);

    /// <summary>
    /// Builds the physical Lucene field name for a field, optionally scoped to a segment.
    /// </summary>
    /// <param name="fieldName">The logical field name.</param>
    /// <param name="fieldValues">The value-type suffix to use (see <see cref="Umbraco.Cms.Search.Provider.Examine.Constants.FieldValues"/>).</param>
    /// <param name="segment">The segment to scope the field name to, or null/empty for no segment.</param>
    /// <returns>The physical Lucene field name.</returns>
    public static string FieldName(string fieldName, string fieldValues, string? segment)
        => $"Field_{fieldName}_{fieldValues}{(segment.IsNullOrWhiteSpace() ? string.Empty : $"_{segment}")}";

    /// <summary>
    /// Builds the name of the additional "queryable keyword" field used for sortable/facetable keyword fields.
    /// </summary>
    /// <param name="fieldName">The physical field name to build the queryable variant of.</param>
    /// <returns>The queryable keyword field name.</returns>
    public static string QueryableKeywordFieldName(string fieldName)
        => $"__Query_{fieldName}";

    /// <summary>
    /// Builds the name of a system field, optionally scoped to a segment.
    /// </summary>
    /// <param name="systemFieldName">The unscoped system field name.</param>
    /// <param name="segment">The segment to scope the field name to, or null/empty for no segment.</param>
    /// <returns>The segment-scoped system field name.</returns>
    public static string SegmentedSystemFieldName(string systemFieldName, string? segment)
        => segment.IsNullOrWhiteSpace() ? systemFieldName : $"{systemFieldName}_{segment}";
}
