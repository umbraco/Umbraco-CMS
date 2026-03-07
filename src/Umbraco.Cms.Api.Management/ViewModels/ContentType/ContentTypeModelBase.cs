using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base model for content types, parameterized by property type and property type container.
/// </summary>
public abstract class ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    /// <summary>
    /// Gets or sets the unique alias (identifier) of the content type.
    /// </summary>
    [Required]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the content type.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the content type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the content type.
    /// </summary>
    [Required]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this content type is allowed to be a root node.
    /// </summary>
    public bool AllowedAsRoot { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content type varies by culture.
    /// </summary>
    public bool VariesByCulture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content type varies by segment, such as language or culture segment.
    /// </summary>
    public bool VariesBySegment { get; set; }

    /// <summary>
    /// Gets or sets a reference to the collection that this content type belongs to, if any.
    /// </summary>
    public ReferenceByIdModel? Collection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this content type is an element.
    /// </summary>
    public bool IsElement { get; set; }

    /// <summary>
    /// Gets or sets the collection of property types associated with the content type.
    /// </summary>
    [Required]
    public IEnumerable<TPropertyType> Properties { get; set; } = Enumerable.Empty<TPropertyType>();

    /// <summary>
    /// Gets or sets the collection of property type containers associated with this content type model.
    /// Each container groups related property types for organizational purposes.
    /// </summary>
    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Enumerable.Empty<TPropertyTypeContainer>();
}
