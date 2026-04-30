namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a request model used for creating a new data type via the management API.
/// </summary>
public class CreateDataTypeRequestModel : DataTypeModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the data type.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the parent entity reference for the data type, identified by ID.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
