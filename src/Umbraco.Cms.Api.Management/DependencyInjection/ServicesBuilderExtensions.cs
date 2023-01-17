﻿using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.Services.Dictionary;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Core.Services.Languages;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class ServicesBuilderExtensions
{
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IJsonPatchService, JsonPatchService>();
        builder.Services.AddTransient<ILanguageService, LanguageService>();
        builder.Services.AddTransient<ILoadDictionaryItemService, LoadDictionaryItemService>();
        builder.Services.AddTransient<ISystemTextJsonSerializer, SystemTextJsonSerializer>();
        builder.Services.AddTransient<IUploadFileService, UploadFileService>();
        builder.Services.AddTransient<IDictionaryService, DictionaryService>();

        // TODO: handle new management API path in core UmbracoRequestPaths (it's a behavioural breaking change so it goes here for now)
        builder.Services.Configure<UmbracoRequestPathsOptions>(options =>
        {
            options.IsBackOfficeRequest = urlPath => urlPath.InvariantStartsWith($"/umbraco/management/api/");
        });

        return builder;
    }
}
