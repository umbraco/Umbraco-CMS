using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Content.Accessors;
using Umbraco.Cms.Api.Content.Json;
using Umbraco.Cms.Api.Content.Rendering;
using Umbraco.Cms.Api.Content.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Content.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddContentApi(this IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IRequestStartItemProvider, RequestStartItemProvider>();
        builder.Services.AddScoped<IOutputExpansionStrategy, RequestContextOutputExpansionStrategy>();
        builder.Services.AddSingleton<IRequestCultureService, RequestCultureService>();
        builder.Services.AddSingleton<IRequestRoutingService, RequestRoutingService>();
        builder.Services.AddSingleton<IRequestPreviewService, RequestPreviewService>();
        builder.Services.AddSingleton<IOutputExpansionStrategyAccessor, RequestContextOutputExpansionStrategyAccessor>();
        builder.Services.AddSingleton<IRequestStartItemProviderAccessor, RequestContextRequestStartItemProviderAccessor>();
        builder.Services.AddSingleton<IApiAccessService, ApiAccessService>();
        builder.Services.AddSingleton<IApiQueryService, ApiQueryService>();

        builder
            .Services
            .ConfigureOptions<ConfigureMvcOptions>()
            .AddControllers()
            .AddJsonOptions(Constants.JsonOptionsNames.ContentApi, options =>
            {
                // all content API specific JSON options go here
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.TypeInfoResolver = new ContentApiJsonTypeResolver();
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return builder;
    }
}
