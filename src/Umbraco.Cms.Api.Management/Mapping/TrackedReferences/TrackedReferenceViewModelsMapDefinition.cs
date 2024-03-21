using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

namespace Umbraco.Cms.Api.Management.Mapping.TrackedReferences;

public class TrackedReferenceViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<RelationItemModel, DocumentReferenceResponseModel>((source, context) => new DocumentReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, MediaReferenceResponseModel>((source, context) => new MediaReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, ReferenceByIdModel>((source, context) => new ReferenceByIdModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, DocumentReferenceResponseModel target, MapperContext context)
    {
        target.ContentTypeAlias = source.ContentTypeAlias;
        target.ContentTypeIcon = source.ContentTypeIcon;
        target.ContentTypeName = source.ContentTypeName;
        target.NodeId = source.NodeKey;
        target.NodeName = source.NodeName;
        target.NodeType = source.NodeType;
        target.NodePublished = source.NodePublished;
    }

    private void Map(RelationItemModel source, MediaReferenceResponseModel target, MapperContext context)
    {
        target.ContentTypeAlias = source.ContentTypeAlias;
        target.ContentTypeIcon = source.ContentTypeIcon;
        target.ContentTypeName = source.ContentTypeName;
        target.NodeId = source.NodeKey;
        target.NodeName = source.NodeName;
        target.NodeType = source.NodeType;
        target.NodePublished = source.NodePublished;
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, ReferenceByIdModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
    }
}
