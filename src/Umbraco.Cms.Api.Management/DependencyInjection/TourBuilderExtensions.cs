using Umbraco.Cms.Api.Management.Mapping.Tour;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TourBuilderExtensions
{
    internal static IUmbracoBuilder AddTours(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TourViewModelsMapDefinition>();

        return builder;
    }
}
