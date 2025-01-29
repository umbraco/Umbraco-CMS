using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class VariantModelBase : IHasCultureAndSegment
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
