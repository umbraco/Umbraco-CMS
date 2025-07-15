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
        mapper.Define<RelationItemModel, MemberReferenceResponseModel>((source, context) => new MemberReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, DocumentTypePropertyTypeReferenceResponseModel>((source, context) => new DocumentTypePropertyTypeReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, MediaTypePropertyTypeReferenceResponseModel>((source, context) => new MediaTypePropertyTypeReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, MemberTypePropertyTypeReferenceResponseModel>((source, context) => new MemberTypePropertyTypeReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, DefaultReferenceResponseModel>((source, context) => new DefaultReferenceResponseModel(), Map);
        mapper.Define<RelationItemModel, ReferenceByIdModel>((source, context) => new ReferenceByIdModel(), Map);
        mapper.Define<Guid, ReferenceByIdModel>((source, context) => new ReferenceByIdModel(), Map);
    }

    // Umbraco.Code.MapAll -Variants
    private void Map(RelationItemModel source, DocumentReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.Published = source.NodePublished;
        target.DocumentType = new TrackedReferenceDocumentType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, MediaReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.MediaType = new TrackedReferenceMediaType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, MemberReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.MemberType = new TrackedReferenceMemberType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, DocumentTypePropertyTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.Alias = source.NodeAlias;
        target.DocumentType = new TrackedReferenceDocumentType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, MediaTypePropertyTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.Alias = source.NodeAlias;
        target.MediaType = new TrackedReferenceMediaType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, MemberTypePropertyTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.Alias = source.NodeAlias;
        target.MemberType = new TrackedReferenceMemberType
        {
            Id = source.ContentTypeKey,
            Alias = source.ContentTypeAlias,
            Icon = source.ContentTypeIcon,
            Name = source.ContentTypeName,
        };
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, DefaultReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
        target.Name = source.NodeName;
        target.Type = source.NodeType;
        target.Icon = source.ContentTypeIcon;
    }

    // Umbraco.Code.MapAll
    private void Map(RelationItemModel source, ReferenceByIdModel target, MapperContext context)
    {
        target.Id = source.NodeKey;
    }

    // Umbraco.Code.MapAll
    private void Map(Guid source, ReferenceByIdModel target, MapperContext context)
    {
        target.Id = source;
    }
}
