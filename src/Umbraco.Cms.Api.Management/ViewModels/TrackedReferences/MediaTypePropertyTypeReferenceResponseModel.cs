namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

    /// <summary>
    /// Represents the response model containing information about a property type reference within a media type.
    /// </summary>
public class MediaTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the referenced media type associated with this property type.
    /// </summary>
    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
