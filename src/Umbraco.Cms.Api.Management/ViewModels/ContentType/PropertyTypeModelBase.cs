namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class PropertyTypeModelBase
{
    public Guid Id { get; set; }

    public Guid? ContainerId { get; set; }

    public int SortOrder { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DataTypeId { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public PropertyTypeValidation Validation { get; set; } = new();

    public PropertyTypeAppearance Appearance { get; set; } = new();
}
