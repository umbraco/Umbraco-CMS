namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents the data required to request moving a media type within the system.
/// </summary>
public class MoveMediaTypeRequestModel
{
    /// <summary>
    /// Gets or sets the target destination, specified by ID, to which the media type will be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
