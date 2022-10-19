using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Content" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(Content))]
[MapperFor(typeof(IContent))]
public sealed class ContentMapper : BaseMapper
{
    public ContentMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Content, NodeDto>(nameof(Content.Id), nameof(NodeDto.NodeId));
        DefineMap<Content, NodeDto>(nameof(Content.Key), nameof(NodeDto.UniqueId));

        DefineMap<Content, ContentVersionDto>(nameof(Content.VersionId), nameof(ContentVersionDto.Id));
        DefineMap<Content, ContentVersionDto>(nameof(Content.Name), nameof(ContentVersionDto.Text));

        DefineMap<Content, NodeDto>(nameof(Content.ParentId), nameof(NodeDto.ParentId));
        DefineMap<Content, NodeDto>(nameof(Content.Level), nameof(NodeDto.Level));
        DefineMap<Content, NodeDto>(nameof(Content.Path), nameof(NodeDto.Path));
        DefineMap<Content, NodeDto>(nameof(Content.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<Content, NodeDto>(nameof(Content.Trashed), nameof(NodeDto.Trashed));

        DefineMap<Content, NodeDto>(nameof(Content.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<Content, NodeDto>(nameof(Content.CreatorId), nameof(NodeDto.UserId));
        DefineMap<Content, ContentDto>(nameof(Content.ContentTypeId), nameof(ContentDto.ContentTypeId));

        DefineMap<Content, ContentVersionDto>(nameof(Content.UpdateDate), nameof(ContentVersionDto.VersionDate));
        DefineMap<Content, DocumentDto>(nameof(Content.Published), nameof(DocumentDto.Published));

        // DefineMap<Content, DocumentDto>(nameof(Content.Name), nameof(DocumentDto.Alias));
        // CacheMap<Content, DocumentDto>(src => src, dto => dto.Newest);
        // DefineMap<Content, DocumentDto>(nameof(Content.Template), nameof(DocumentDto.TemplateId));
    }
}
