namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Icon { get; set; } = string.Empty;

    public bool AllowedAsRoot { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public bool IsElement { get; set; }

    public IEnumerable<TPropertyType> Properties { get; set; } = Array.Empty<TPropertyType>();

    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Array.Empty<TPropertyTypeContainer>();

    public IEnumerable<ContentTypeSort> AllowedContentTypes { get; set; } = Array.Empty<ContentTypeSort>();

    public IEnumerable<ContentTypeComposition> Compositions { get; set; } = Array.Empty<ContentTypeComposition>();
}
