using System;
using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Infrastructure.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Media"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IMedia))]
    [MapperFor(typeof(Media))]
    public sealed class MediaMapper : BaseMapper
    {
        public MediaMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<Media, NodeDto>(nameof(Media.Id), nameof(NodeDto.NodeId));
            DefineMap<Media, NodeDto>(nameof(Media.Key), nameof(NodeDto.UniqueId));

            DefineMap<Content, ContentVersionDto>(nameof(Content.VersionId), nameof(ContentVersionDto.Id));

            DefineMap<Media, NodeDto>(nameof(Media.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<Media, NodeDto>(nameof(Media.Level), nameof(NodeDto.Level));
            DefineMap<Media, NodeDto>(nameof(Media.ParentId), nameof(NodeDto.ParentId));
            DefineMap<Media, NodeDto>(nameof(Media.Path), nameof(NodeDto.Path));
            DefineMap<Media, NodeDto>(nameof(Media.SortOrder), nameof(NodeDto.SortOrder));
            DefineMap<Media, NodeDto>(nameof(Media.Name), nameof(NodeDto.Text));
            DefineMap<Media, NodeDto>(nameof(Media.Trashed), nameof(NodeDto.Trashed));
            DefineMap<Media, NodeDto>(nameof(Media.CreatorId), nameof(NodeDto.UserId));
            DefineMap<Media, ContentDto>(nameof(Media.ContentTypeId), nameof(ContentDto.ContentTypeId));
            DefineMap<Media, ContentVersionDto>(nameof(Media.UpdateDate), nameof(ContentVersionDto.VersionDate));
        }
    }
}
