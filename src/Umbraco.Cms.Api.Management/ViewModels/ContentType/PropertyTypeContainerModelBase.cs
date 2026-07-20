using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Represents the base model for a container that holds property types within a content type.
/// </summary>
public abstract class PropertyTypeContainerModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the property type container.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the reference to the parent property type container, if any.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets the display name of this property type container.
    /// </summary>
    public string? Name { get; set; }

    // NOTE: This needs to be a string because it can be anything in the future (= not necessarily limited to "tab" or "group")
    /// <summary>
    /// Gets or sets the type of the property container, such as "tab" or "group". This value is a string to allow for future extensibility.
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order in which the property type container is sorted.
    /// </summary>
    public int SortOrder { get; set; }
}
