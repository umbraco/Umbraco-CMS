namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents the data required to create a copy of an existing media type.
/// </summary>
public class CopyMediaTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target media type, referenced by its ID.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
