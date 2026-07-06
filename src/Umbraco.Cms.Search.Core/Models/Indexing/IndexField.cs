namespace Umbraco.Cms.Search.Core.Models.Indexing;

public record IndexField(string FieldName, IndexValue Value, string? Culture, string? Segment);
