using Umbraco.Cms.Api.Management.Mapping.Entity;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class EntityBuilderExtensions
{
    internal static IUmbracoBuilder AddEntitys(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ItemTypeMapDefinition>();

        return builder;
    }
}
