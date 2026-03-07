namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

    /// <summary>
    /// Represents a response model containing information about a media item referenced in tracked references.
    /// </summary>
public class MediaReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the type of the referenced media.
    /// </summary>
    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
