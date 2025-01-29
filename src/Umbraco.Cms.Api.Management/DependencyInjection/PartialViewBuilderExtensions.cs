using Umbraco.Cms.Api.Management.Mapping.PartialView;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class PartialViewBuilderExtensions
{
    internal static IUmbracoBuilder AddPartialViews(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<PartialViewViewModelsMapDefinition>();

        return builder;
    }
}
