namespace Umbraco.Cms.Core.PropertyEditors;

public sealed class IndexValue
{
    public required string? Culture { get; set; }

    public required string FieldName { get; set; }

    public required IEnumerable<object?> Values { get; set; }
}
