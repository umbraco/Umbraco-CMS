namespace Umbraco.Cms.Api.Management.ViewModels.Media;

/// <summary>
/// Represents the data required to move one or more media items within the media library.
/// </summary>
public class MoveMediaRequestModel
{
    /// <summary>
    /// Gets or sets the reference to the target location where the media item will be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
