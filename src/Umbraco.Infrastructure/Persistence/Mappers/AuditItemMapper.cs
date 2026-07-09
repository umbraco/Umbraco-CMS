using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Provides mapping configuration between the <see cref="AuditItem"/> entity and its corresponding database table for ORM operations within the Umbraco infrastructure.
/// </summary>
[MapperFor(typeof(AuditItem))]
[MapperFor(typeof(IAuditItem))]
public sealed class AuditItemMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditItemMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">A lazily-initialized <see cref="ISqlContext"/> providing SQL context for the mapper.</param>
    /// <param name="maps">The <see cref="MapperConfigurationStore"/> containing mapping configurations.</param>
    public AuditItemMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<AuditItem, LogDto>(nameof(AuditItem.Id), nameof(LogDto.NodeId));
        DefineMap<AuditItem, LogDto>(nameof(AuditItem.CreateDate), nameof(LogDto.Datestamp));
        DefineMap<AuditItem, LogDto>(nameof(AuditItem.UserId), nameof(LogDto.UserId));

        // we cannot map that one - because AuditType is an enum but Header is a string
        // DefineMap<AuditItem, LogDto>(nameof(AuditItem.AuditType), nameof(LogDto.Header));
        DefineMap<AuditItem, LogDto>(nameof(AuditItem.Comment), nameof(LogDto.Comment));
    }
}
