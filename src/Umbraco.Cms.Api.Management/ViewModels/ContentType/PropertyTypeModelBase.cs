using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Represents the base model for a property type in a content type.
/// </summary>
public abstract class PropertyTypeModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the property type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the container that organizes this property type.
    /// </summary>
    public ReferenceByIdModel? Container { get; set; }

    /// <summary>
    /// Gets or sets the sort order of the property type.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the alias of the property type.
    /// </summary>
    [Required]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the property type.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual description of the property type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the data type reference associated with this property type.
    /// </summary>
    public ReferenceByIdModel DataType { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the value of the property type varies by culture.
    /// </summary>
    public bool VariesByCulture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property type varies by segment, such as language or culture segment.
    /// </summary>
    public bool VariesBySegment { get; set; }

    /// <summary>Gets or sets the validation settings for the property type.</summary>
    public PropertyTypeValidation Validation { get; set; } = new();

    /// <summary>
    /// Gets or sets the UI appearance settings for the property type.
    /// </summary>
    public PropertyTypeAppearance Appearance { get; set; } = new();
}
