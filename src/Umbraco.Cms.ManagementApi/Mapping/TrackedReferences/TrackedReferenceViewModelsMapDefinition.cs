using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.TrackedReferences;

namespace Umbraco.Cms.ManagementApi.Mapping.TrackedReferences;

public class TrackedReferenceViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<RelationItemModel, RelationItemViewModel>((source, context) => new RelationItemViewModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, RelationItemViewModel target, MapperContext context)
    {
        target.ContentTypeAlias = source.ContentTypeAlias;
        target.ContentTypeIcon = source.ContentTypeIcon;
        target.ContentTypeName = source.ContentTypeName;
        target.NodeKey = source.NodeKey;
        target.NodeName = source.NodeName;
        target.NodeType = source.NodeType;
        target.RelationTypeIsBidirectional = source.RelationTypeIsBidirectional;
        target.RelationTypeIsDependency = source.RelationTypeIsDependency;
        target.RelationTypeName = source.RelationTypeName;
    }

}
