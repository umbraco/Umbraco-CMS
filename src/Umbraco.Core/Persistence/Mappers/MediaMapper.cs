using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Models.Media"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(IMedia))]
    [MapperFor(typeof(Umbraco.Core.Models.Media))]
    public sealed class MediaMapper : BaseMapper
    {
        public MediaMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Id), nameof(NodeDto.NodeId));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Key), nameof(NodeDto.UniqueId));

            DefineMap<Content, ContentVersionDto>(nameof(Content.VersionId), nameof(ContentVersionDto.Id));

            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Level), nameof(NodeDto.Level));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.ParentId), nameof(NodeDto.ParentId));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Path), nameof(NodeDto.Path));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.SortOrder), nameof(NodeDto.SortOrder));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Name), nameof(NodeDto.Text));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.Trashed), nameof(NodeDto.Trashed));
            DefineMap<Models.Media, NodeDto>(nameof(Models.Media.CreatorId), nameof(NodeDto.UserId));
            DefineMap<Models.Media, ContentDto>(nameof(Models.Media.ContentTypeId), nameof(ContentDto.ContentTypeId));
            DefineMap<Models.Media, ContentVersionDto>(nameof(Models.Media.UpdateDate), nameof(ContentVersionDto.VersionDate));
        }
    }
}
