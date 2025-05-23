using Umbraco.Cms.Api.Management.Mapping.LogViewer;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class LogViewerBuilderExtensions
{
    internal static IUmbracoBuilder AddLogViewer(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<LogViewerViewModelMapDefinition>();

        return builder;
    }
}
