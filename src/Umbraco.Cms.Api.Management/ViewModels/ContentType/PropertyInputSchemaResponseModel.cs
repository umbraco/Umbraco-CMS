namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Represents property input schema information in API responses.
/// </summary>
public class PropertyInputSchemaResponseModel
{
    /// <summary>
    /// Gets or sets the property alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the unique key of the data type used by this property.
    /// </summary>
    public required Guid DataTypeId { get; set; }

    /// <summary>
    /// Gets or sets the property editor alias.
    /// </summary>
    public required string EditorAlias { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a value is required for this property.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the content variation setting for this property.
    /// </summary>
    public required string Variations { get; set; }
}
