using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ValueModelBase : IHasCultureAndSegment
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    [Required]
    public string Alias { get; set; } = string.Empty;

    public object? Value { get; set; }
}
