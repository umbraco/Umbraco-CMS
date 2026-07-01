namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents the data required to create a copy of an existing data type.
/// </summary>
public class CopyDataTypeRequestModel
{
    /// <summary>
    /// Gets or sets the reference to the target location where the data type will be copied.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
