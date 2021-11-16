using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(ISimpleContentType))]
    [MapperFor(typeof(SimpleContentType))]
    public sealed class SimpleContentTypeMapper : BaseMapper
    {
        public SimpleContentTypeMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Id), nameof(NodeDto.NodeId));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Level), nameof(NodeDto.Level));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.ParentId), nameof(NodeDto.ParentId));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Path), nameof(NodeDto.Path));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.SortOrder), nameof(NodeDto.SortOrder));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Name), nameof(NodeDto.Text));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Trashed), nameof(NodeDto.Trashed));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.Key), nameof(NodeDto.UniqueId));
            DefineMap<ContentType, NodeDto>(nameof(ContentType.CreatorId), nameof(NodeDto.UserId));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.Alias), nameof(ContentTypeDto.Alias));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.AllowedAsRoot), nameof(ContentTypeDto.AllowAtRoot));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.Description), nameof(ContentTypeDto.Description));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.Icon), nameof(ContentTypeDto.Icon));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.IsContainer), nameof(ContentTypeDto.IsContainer));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.IsElement), nameof(ContentTypeDto.IsElement));
            DefineMap<ContentType, ContentTypeDto>(nameof(ContentType.Thumbnail), nameof(ContentTypeDto.Thumbnail));
        }
    }
}
