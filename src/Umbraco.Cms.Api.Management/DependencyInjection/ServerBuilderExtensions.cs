using Umbraco.Cms.Api.Management.Mapping.Server;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ServerBuilderExtensions
{
    internal static IUmbracoBuilder AddServer(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ServerConfigurationMapDefinition>();

        return builder;
    }
}
