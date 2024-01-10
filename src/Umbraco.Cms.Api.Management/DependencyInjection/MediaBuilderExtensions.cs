﻿using Microsoft.Extensions.DependencyInjection;
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
        builder.Services.AddTransient<IMediaPresentationModelFactory, MediaPresentationModelFactory>();
        builder.Services.AddTransient<IMediaEditingPresentationFactory, MediaEditingPresentationFactory>();
        builder.Services.AddTransient<IUrlAssembler, DefaultUrlAssembler>();
        builder.Services.AddScoped<IAbsoluteUrlBuilder, DefaultAbsoluteUrlBuilder>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MediaMapDefinition>();

        return builder;
    }
}
