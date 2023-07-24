namespace Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;

public abstract class PropertyTypeModelBase
{
    public Guid Key { get; set; }

    public Guid? ContainerKey { get; set; }

    public int SortOrder { get; set; }

    public required string Alias { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid DataTypeKey { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public required PropertyTypeValidation Validation { get; set; }

    public required PropertyTypeAppearance Appearance { get; set; }
}
