using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Media" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(IMedia))]
[MapperFor(typeof(Core.Models.Media))]
public sealed class MediaMapper : BaseMapper
{
    public MediaMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Id), nameof(NodeDto.NodeId));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Key), nameof(NodeDto.UniqueId));

        DefineMap<Content, ContentVersionDto>(nameof(Content.VersionId), nameof(ContentVersionDto.Id));

        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Level), nameof(NodeDto.Level));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.ParentId), nameof(NodeDto.ParentId));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Path), nameof(NodeDto.Path));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Name), nameof(NodeDto.Text));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.Trashed), nameof(NodeDto.Trashed));
        DefineMap<Core.Models.Media, NodeDto>(nameof(Core.Models.Media.CreatorId), nameof(NodeDto.UserId));
        DefineMap<Core.Models.Media, ContentDto>(nameof(Core.Models.Media.ContentTypeId), nameof(ContentDto.ContentTypeId));
        DefineMap<Core.Models.Media, ContentVersionDto>(nameof(Core.Models.Media.UpdateDate), nameof(ContentVersionDto.VersionDate));
    }
}
