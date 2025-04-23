namespace Umbraco.Cms.Core.Models.ContentEditing;

public class PropertyValueModel
{
    public required string Alias { get; set; }

    public required object? Value { get; set; }

    public string? Culture { get; set; }

    public string? Segment { get; set; }
}
