namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public abstract class PropertyTypeModelBase
{
    public Guid Key { get; set; }

    public Guid? ContainerKey { get; set; }

    public int SortOrder { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DataTypeKey { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public PropertyTypeValidation Validation { get; set; } = new();

    public PropertyTypeAppearance Appearance { get; set; } = new();
}
