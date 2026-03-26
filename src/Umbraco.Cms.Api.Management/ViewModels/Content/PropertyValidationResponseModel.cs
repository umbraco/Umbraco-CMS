namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Represents a model containing the results of validating a content property.
/// </summary>
public class PropertyValidationResponseModel
{
    /// <summary>
    /// Gets or sets the collection of validation messages associated with the property.
    /// </summary>
    public string[] Messages { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the alias of the property for which the validation response applies.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the culture associated with the property validation response.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the segment associated with the property validation response.
    /// </summary>
    public string? Segment { get; set; }
}
