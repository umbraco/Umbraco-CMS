using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Media;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.PropertyEditors;
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
        builder.Services.AddTransient<IReziseImageUrlFactory>(serviceProvider => new ReziseImageUrlFactory(
            serviceProvider.GetRequiredService<IImageUrlGenerator>(),
            serviceProvider.GetRequiredService<IOptions<ContentSettings>>(),
            serviceProvider.GetRequiredService<MediaUrlGeneratorCollection>(),
            serviceProvider.GetRequiredService<IAbsoluteUrlBuilder>()));
        builder.Services.AddScoped<IAbsoluteUrlBuilder, DefaultAbsoluteUrlBuilder>();
        builder.Services.AddTransient<IMediaCollectionPresentationFactory, MediaCollectionPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaMapDefinition>();

        return builder;
    }
}
