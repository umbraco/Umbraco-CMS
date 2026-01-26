using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Variant patch model for documents.
/// </summary>
public class DocumentVariantPatchModel
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    /// <summary>
    /// New name for this variant. Required if variant is specified.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}
