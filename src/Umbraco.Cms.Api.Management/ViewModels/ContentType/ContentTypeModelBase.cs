using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    [Required]
    public string Alias { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string Icon { get; set; } = string.Empty;

    public bool AllowedAsRoot { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public ReferenceByIdModel? Collection { get; set; }

    public bool IsElement { get; set; }

    [Required]
    public IEnumerable<TPropertyType> Properties { get; set; } = Enumerable.Empty<TPropertyType>();

    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Enumerable.Empty<TPropertyTypeContainer>();
}
