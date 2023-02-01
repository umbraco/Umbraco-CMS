namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeViewModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeViewModelBase
    where TPropertyTypeContainer : PropertyTypeContainerViewModelBase
{
    public Guid Key { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool AllowedAsRoot { get; set; }

    public bool VariesByCulture { get; set; }

    public bool IsElement { get; set; }

    public IEnumerable<TPropertyType> Properties { get; set; } = Array.Empty<TPropertyType>();

    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Array.Empty<TPropertyTypeContainer>();
}
