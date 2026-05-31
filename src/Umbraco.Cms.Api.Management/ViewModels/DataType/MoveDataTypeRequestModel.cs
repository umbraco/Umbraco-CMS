namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Request model for moving a data type.
/// </summary>
public class MoveDataTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target destination for the move operation, referenced by ID.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
