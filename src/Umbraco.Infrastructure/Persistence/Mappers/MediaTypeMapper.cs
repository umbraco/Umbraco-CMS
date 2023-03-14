using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="MediaType" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(IMediaType))]
[MapperFor(typeof(MediaType))]
public sealed class MediaTypeMapper : BaseMapper
{
    public MediaTypeMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Id), nameof(NodeDto.NodeId));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Level), nameof(NodeDto.Level));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.ParentId), nameof(NodeDto.ParentId));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Path), nameof(NodeDto.Path));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Name), nameof(NodeDto.Text));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Trashed), nameof(NodeDto.Trashed));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.Key), nameof(NodeDto.UniqueId));
        DefineMap<MediaType, NodeDto>(nameof(MediaType.CreatorId), nameof(NodeDto.UserId));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.Alias), nameof(ContentTypeDto.Alias));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.AllowedAsRoot), nameof(ContentTypeDto.AllowAtRoot));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.Description), nameof(ContentTypeDto.Description));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.Icon), nameof(ContentTypeDto.Icon));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.IsContainer), nameof(ContentTypeDto.IsContainer));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.IsElement), nameof(ContentTypeDto.IsElement));
        DefineMap<MediaType, ContentTypeDto>(nameof(MediaType.Thumbnail), nameof(ContentTypeDto.Thumbnail));
    }
}
