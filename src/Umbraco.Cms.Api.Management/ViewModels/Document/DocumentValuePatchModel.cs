using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Value patch model for document properties.
/// </summary>
public class DocumentValuePatchModel
{
    [Required]
    public string Alias { get; set; } = string.Empty;

    public string? Culture { get; set; }

    public string? Segment { get; set; }

    /// <summary>
    /// New value. Null explicitly clears the value.
    /// </summary>
    public object? Value { get; set; }
}
