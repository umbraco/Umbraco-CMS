namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class PropertyTypeViewModelBase
{
    public Guid Key { get; set; }

    public Guid? ContainerKey { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DataTypeKey { get; set; }

    public PropertyTypeValidation Validation { get; set; } = new();
}
