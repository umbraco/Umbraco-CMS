namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Base model for property types used in content type editing.
/// </summary>
public abstract class PropertyTypeModelBase
{
    /// <summary>
    ///     Gets or sets the unique key of the property type.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the container (tab or group) that this property belongs to.
    /// </summary>
    public Guid? ContainerKey { get; set; }

    /// <summary>
    ///     Gets or sets the sort order of the property type within its container.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the property type.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the property type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the property type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the key of the data type used by this property type.
    /// </summary>
    public Guid DataTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the property value varies by culture.
    /// </summary>
    public bool VariesByCulture { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the property value varies by segment.
    /// </summary>
    public bool VariesBySegment { get; set; }

    /// <summary>
    ///     Gets or sets the validation settings for the property type.
    /// </summary>
    public PropertyTypeValidation Validation { get; set; } = new();

    /// <summary>
    ///     Gets or sets the appearance settings for the property type.
    /// </summary>
    public PropertyTypeAppearance Appearance { get; set; } = new();
}
