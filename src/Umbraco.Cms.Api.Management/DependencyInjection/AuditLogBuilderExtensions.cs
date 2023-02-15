using Umbraco.Cms.Api.Management.Mapping.AuditLog;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class AuditLogBuilderExtensions
{
    internal static IUmbracoBuilder AddAuditLogs(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<AuditLogViewModelMapDefinition>();

        return builder;
    }
}
