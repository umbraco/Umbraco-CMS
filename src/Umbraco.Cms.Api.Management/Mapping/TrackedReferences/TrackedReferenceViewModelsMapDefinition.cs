using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

namespace Umbraco.Cms.Api.Management.Mapping.TrackedReferences;

public class TrackedReferenceViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<RelationItemModel, RelationItemResponseModel>((source, context) => new RelationItemResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, RelationItemResponseModel target, MapperContext context)
    {
        target.ContentTypeAlias = source.ContentTypeAlias;
        target.ContentTypeIcon = source.ContentTypeIcon;
        target.ContentTypeName = source.ContentTypeName;
        target.NodeId = source.NodeKey;
        target.NodeName = source.NodeName;
        target.NodeType = source.NodeType;
        target.RelationTypeIsBidirectional = source.RelationTypeIsBidirectional;
        target.RelationTypeIsDependency = source.RelationTypeIsDependency;
        target.RelationTypeName = source.RelationTypeName;
        target.NodePublished = source.NodePublished;
    }

}
