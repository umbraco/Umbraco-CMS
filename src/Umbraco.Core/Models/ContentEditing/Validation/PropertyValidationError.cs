namespace Umbraco.Cms.Core.Models.ContentEditing.Validation;

public class PropertyValidationError
{
    public required string JsonPath { get; init; }

    public required string[] ErrorMessages { get; init; }

    public required string Alias { get; set; }

    public required string? Culture { get; set; }

    public required string? Segment { get; set; }
}
