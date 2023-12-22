using Umbraco.Cms.Api.Management.Mapping.Culture;
using Umbraco.Cms.Api.Management.Mapping.DynamicRoot;
using Umbraco.Cms.Api.Management.Mapping.Language;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DynamicRootBuilderExtensions
{
    internal static IUmbracoBuilder AddDynamicRoot(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<DynamicRootMapDefinition>();

        return builder;
    }
}
