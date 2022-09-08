using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.New.Cms.Core.Models.TrackedReferences;

namespace Umbraco.New.Cms.Infrastructure.Persistence.Mappers;

public class RelationModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<RelationItemDto, RelationModel>((source, context) => new RelationModel(), Map);
    }

    private void Map(RelationItemDto source, RelationModel target, MapperContext context)
    {
        target.NodeKey = source.ChildNodeKey;
        target.NodeType = ObjectTypes.GetUdiType(source.ChildNodeObjectType);
        target.NodeName = source.ChildNodeName;
        target.RelationTypeName = source.RelationTypeName;
        target.RelationTypeIsBidirectional = source.RelationTypeIsBidirectional;
        target.RelationTypeIsDependency = source.RelationTypeIsDependency;
        target.ContentTypeAlias = source.ChildContentTypeAlias;
        target.ContentTypeIcon = source.ChildContentTypeIcon;
        target.ContentTypeName = source.ChildContentTypeName;
    }
}
