namespace Umbraco.New.Cms.Core.Models.TrackedReferences;

// TODO V13: Renamed to RelationItem when we get rid of RelationItem in new backoffice
public class RelationItemModel
{
    public Guid NodeKey { get; set; }

    public string? NodeName { get; set; }

    public string? NodeType { get; set; }

    public string? ContentTypeIcon { get; set; }

    public string? ContentTypeAlias { get; set; }

    public string? ContentTypeName { get; set; }

    public string? RelationTypeName { get; set; }

    public bool RelationTypeIsBidirectional { get; set; }

    public bool RelationTypeIsDependency { get; set; }
}
