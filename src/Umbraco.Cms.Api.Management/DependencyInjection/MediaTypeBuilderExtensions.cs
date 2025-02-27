using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.MediaType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class MediaTypeBuilderExtensions
{
    internal static IUmbracoBuilder AddMediaTypes(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IMediaTypeEditingPresentationFactory, MediaTypeEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaTypeMapDefinition>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaTypeCompositionMapDefinition>();

        return builder;
    }
}
