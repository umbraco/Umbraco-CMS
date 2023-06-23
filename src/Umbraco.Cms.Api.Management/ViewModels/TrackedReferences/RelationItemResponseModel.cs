namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class RelationItemResponseModel
{
    public Guid NodeId { get; set; }

    public string? NodeName { get; set; }

    public string? NodeType { get; set; }

    public bool? NodePublished { get; set; }

    public string? ContentTypeIcon { get; set; }

    public string? ContentTypeAlias { get; set; }

    public string? ContentTypeName { get; set; }

    public string? RelationTypeName { get; set; }

    public bool RelationTypeIsBidirectional { get; set; }

    public bool RelationTypeIsDependency { get; set; }

}
