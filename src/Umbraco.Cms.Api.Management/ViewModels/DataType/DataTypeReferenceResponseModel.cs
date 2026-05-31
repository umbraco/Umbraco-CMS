namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents the response model that contains information referencing a specific data type.
/// </summary>
public class DataTypeReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the reference to the content type associated with this data type.
    /// </summary>
    public required DataTypeContentTypeReferenceModel ContentType { get; init; }

    /// <summary>
    /// Gets or sets the collection of data type property references.
    /// </summary>
    public required IEnumerable<DataTypePropertyReferenceViewModel> Properties { get; init; }
}
