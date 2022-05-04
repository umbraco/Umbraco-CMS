using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(AuditItem))]
[MapperFor(typeof(IAuditItem))]
public sealed class AuditItemMapper : BaseMapper
{
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
