using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Media;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class MediaBuilderExtensions
{
    internal static IUmbracoBuilder AddMedia(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IMediaPresentationModelFactory, MediaPresentationModelFactory>();
        builder.Services.AddTransient<IMediaEditingPresentationFactory, MediaEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaMapDefinition>();

        return builder;
    }
}
