using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides functionality to map properties of Umbraco entities to their corresponding database columns.
/// </summary>
[MapperFor(typeof(IUmbracoEntity))]
public sealed class UmbracoEntityMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoEntityMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded SQL context used for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
    public UmbracoEntityMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
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
