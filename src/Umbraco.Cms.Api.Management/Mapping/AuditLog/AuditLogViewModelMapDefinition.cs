using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.AuditLog;

public class AuditLogViewModelMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<IAuditItem, AuditLogByTypeViewModel>((source, context) => new AuditLogByTypeViewModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IAuditItem source, AuditLogByTypeViewModel target, MapperContext context)
    {
        target.Comment = source.Comment;
        target.EntityType = source.EntityType;
        target.LogType = source.AuditType; // fixme
        target.Parameters = source.Parameters;
        target.Timestamp = source.CreateDate; // fixme
        target.UserId = source.UserId;
    }
}
