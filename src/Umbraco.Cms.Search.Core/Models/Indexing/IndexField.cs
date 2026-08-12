namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Represents a single field and its value(s) to write to a search index, for a given variant.
/// </summary>
/// <param name="FieldName">The name of the field.</param>
/// <param name="Value">The value(s) to index for this field.</param>
/// <param name="Culture">The culture this field value applies to, or null for invariant.</param>
/// <param name="Segment">The segment this field value applies to, or null for unsegmented.</param>
public record IndexField(string FieldName, IndexValue Value, string? Culture, string? Segment);
