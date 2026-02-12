namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Represents content type input schema information in API responses.
/// </summary>
public class ContentTypeInputSchemaResponseModel
{
    /// <summary>
    /// Gets or sets the unique key of the content type.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the content type alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets all properties for this content type.
    /// </summary>
    public required IEnumerable<PropertyInputSchemaResponseModel> Properties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content type is an element type.
    /// </summary>
    public bool IsElement { get; set; }

    /// <summary>
    /// Gets or sets the content variation setting for this content type.
    /// </summary>
    public required string Variations { get; set; }
}
