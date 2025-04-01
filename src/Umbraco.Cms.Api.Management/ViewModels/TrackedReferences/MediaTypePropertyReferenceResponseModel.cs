namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MediaTypePropertyReferenceResponseModel : ContentTypePropertyReferenceResponseModel
{
    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
