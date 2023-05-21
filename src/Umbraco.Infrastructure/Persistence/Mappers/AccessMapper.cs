using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(PublicAccessEntry))]
public sealed class AccessMapper : BaseMapper
{
    public AccessMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.Key), nameof(AccessDto.Id));
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.LoginNodeId), nameof(AccessDto.LoginNodeId));
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.NoAccessNodeId), nameof(AccessDto.NoAccessNodeId));
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.ProtectedNodeId), nameof(AccessDto.NodeId));
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.CreateDate), nameof(AccessDto.CreateDate));
        DefineMap<PublicAccessEntry, AccessDto>(nameof(PublicAccessEntry.UpdateDate), nameof(AccessDto.UpdateDate));
    }
}
