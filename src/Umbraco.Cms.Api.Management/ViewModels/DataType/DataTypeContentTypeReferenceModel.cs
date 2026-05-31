namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a reference to a content type that is associated with a specific data type in the management API.
/// </summary>
public class DataTypeContentTypeReferenceModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the content type referenced by this model.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the referenced content type, such as document, media, or member.
    /// </summary>
    public required string? Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the referenced content type.
    /// </summary>
    public required string? Name { get; set; }

    /// <summary>
    /// Gets or sets the icon representing the referenced content type.
    /// </summary>
    public required string? Icon { get; set; }
}
