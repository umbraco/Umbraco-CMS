namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MediaReferenceResponseModel : IReferenceResponseModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public TrackedReferenceMediaType MediaType { get; set; } = new();
}
