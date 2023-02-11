using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Serialization;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Core.Services.Languages;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class ServicesBuilderExtensions
{
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IJsonPatchService, JsonPatchService>();
        builder.Services.AddTransient<ILanguageService, LanguageService>();
        builder.Services.AddTransient<ILoadDictionaryItemService, LoadDictionaryItemService>();
        builder.Services.AddTransient<ISystemTextJsonSerializer, SystemTextJsonSerializer>();
        builder.Services.AddTransient<IUploadFileService, UploadFileService>();

        return builder;
    }
}
