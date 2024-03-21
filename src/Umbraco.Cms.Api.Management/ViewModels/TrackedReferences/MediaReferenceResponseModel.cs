namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class MediaReferenceResponseModel
{
    public Guid NodeId { get; set; }

    public string? NodeName { get; set; }

    public string? NodeType { get; set; }

    public bool? NodePublished { get; set; }

    public string? ContentTypeIcon { get; set; }

    public string? ContentTypeAlias { get; set; }

    public string? ContentTypeName { get; set; }
}
