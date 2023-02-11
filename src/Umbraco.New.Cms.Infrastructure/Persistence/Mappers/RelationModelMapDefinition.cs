using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.New.Cms.Infrastructure.Persistence.Mappers;

public class RelationModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<RelationItemDto, RelationItemModel>((source, context) => new RelationItemModel(), Map);
    }

    private void Map(RelationItemDto source, RelationItemModel target, MapperContext context)
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
