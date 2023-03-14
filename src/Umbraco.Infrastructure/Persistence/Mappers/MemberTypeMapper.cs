using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="MemberType" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(MemberType))]
[MapperFor(typeof(IMemberType))]
public sealed class MemberTypeMapper : BaseMapper
{
    public MemberTypeMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Id), nameof(NodeDto.NodeId));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.CreateDate), nameof(NodeDto.CreateDate));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Level), nameof(NodeDto.Level));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.ParentId), nameof(NodeDto.ParentId));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Path), nameof(NodeDto.Path));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.SortOrder), nameof(NodeDto.SortOrder));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Name), nameof(NodeDto.Text));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Trashed), nameof(NodeDto.Trashed));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.Key), nameof(NodeDto.UniqueId));
        DefineMap<MemberType, NodeDto>(nameof(MemberType.CreatorId), nameof(NodeDto.UserId));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.Alias), nameof(ContentTypeDto.Alias));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.AllowedAsRoot), nameof(ContentTypeDto.AllowAtRoot));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.Description), nameof(ContentTypeDto.Description));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.Icon), nameof(ContentTypeDto.Icon));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.IsContainer), nameof(ContentTypeDto.IsContainer));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.IsElement), nameof(ContentTypeDto.IsElement));
        DefineMap<MemberType, ContentTypeDto>(nameof(MemberType.Thumbnail), nameof(ContentTypeDto.Thumbnail));
    }
}
