using Umbraco.Cms.Api.Management.Mapping.Abstract;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class AbstractMappingBuilderExtensions
{
    internal static IUmbracoBuilder AddAbstractMappers(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TracksTrashingMapDefinition>();

        return builder;
    }
}
