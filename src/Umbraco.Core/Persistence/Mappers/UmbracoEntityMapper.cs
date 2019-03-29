using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof (IUmbracoEntity))]
    public sealed class UmbracoEntityMapper : BaseMapper
    {
        public UmbracoEntityMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Id), nameof(NodeDto.NodeId));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Level), nameof(NodeDto.Level));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.ParentId), nameof(NodeDto.ParentId));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Path), nameof(NodeDto.Path));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.SortOrder), nameof(NodeDto.SortOrder));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Name), nameof(NodeDto.Text));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Trashed), nameof(NodeDto.Trashed));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.Key), nameof(NodeDto.UniqueId));
            DefineMap<IUmbracoEntity, NodeDto>(nameof(IUmbracoEntity.CreatorId), nameof(NodeDto.UserId));
        }
    }
}
