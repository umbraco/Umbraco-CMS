using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class PropertyTypeModelBase
{
    public Guid Id { get; set; }

    public ReferenceByIdModel? Container { get; set; }

    public int SortOrder { get; set; }

    [Required]
    public string Alias { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ReferenceByIdModel DataType { get; set; } = new();

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public PropertyTypeValidation Validation { get; set; } = new();

    public PropertyTypeAppearance Appearance { get; set; } = new();
}
