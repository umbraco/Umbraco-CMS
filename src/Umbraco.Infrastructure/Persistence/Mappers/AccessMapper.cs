using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping functionality for Access entities between the database and domain models in the persistence layer.
/// </summary>
[MapperFor(typeof(PublicAccessEntry))]
public sealed class AccessMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily-initialized SQL context for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
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
