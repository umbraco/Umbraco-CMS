namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MediaTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
