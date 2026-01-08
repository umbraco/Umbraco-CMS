using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Media;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class MediaBuilderExtensions
{
    internal static IUmbracoBuilder AddMedia(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IMediaPresentationFactory, MediaPresentationFactory>();
        builder.Services.AddTransient<IMediaEditingPresentationFactory, MediaEditingPresentationFactory>();
        builder.Services.AddTransient<IUrlAssembler, DefaultUrlAssembler>();
        builder.Services.AddTransient<IMediaUrlFactory, MediaUrlFactory>();
        builder.Services.AddTransient<IReziseImageUrlFactory, ReziseImageUrlFactory>();
        builder.Services.AddScoped<IAbsoluteUrlBuilder, DefaultAbsoluteUrlBuilder>();
        builder.Services.AddTransient<IMediaCollectionPresentationFactory, MediaCollectionPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaMapDefinition>();

        return builder;
    }
}
