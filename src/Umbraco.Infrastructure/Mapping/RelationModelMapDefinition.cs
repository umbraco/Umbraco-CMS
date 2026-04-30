using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Infrastructure.Mapping;

/// <summary>
/// Provides mapping configuration for the <see cref="RelationModel"/> entity within the Umbraco infrastructure.
/// </summary>
public class RelationModelMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mappings for relation models within Umbraco, specifically defining how <see cref="RelationItemDto"/> instances are mapped to <see cref="RelationItemModel"/>.
    /// </summary>
    /// <param name="mapper">The Umbraco mapper used to register the mapping definitions.</param>
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
        target.ContentTypeKey = source.ChildContentTypeKey;
        target.ContentTypeAlias = source.ChildContentTypeAlias;
        target.ContentTypeIcon = source.ChildContentTypeIcon;
        target.ContentTypeName = source.ChildContentTypeName;

        target.NodePublished = source.ChildNodePublished;
    }
}
