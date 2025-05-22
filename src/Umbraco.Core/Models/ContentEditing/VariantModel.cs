namespace Umbraco.Cms.Core.Models.ContentEditing;

public class VariantModel
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    public required string Name { get; set; }
}
