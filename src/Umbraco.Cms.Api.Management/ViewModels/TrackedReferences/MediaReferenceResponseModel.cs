namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MediaReferenceResponseModel : ReferenceResponseModel
{
    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
