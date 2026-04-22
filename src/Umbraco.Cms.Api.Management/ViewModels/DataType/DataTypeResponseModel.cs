namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a response model for a data type in the Umbraco CMS Management API.
/// </summary>
public class DataTypeResponseModel : DataTypeModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the data type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this data type can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the start nodes restriction can be ignored for this data type.
    /// </summary>
    public bool CanIgnoreStartNodes { get; set; }
}
