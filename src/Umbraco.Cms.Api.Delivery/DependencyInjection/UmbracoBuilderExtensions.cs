using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Delivery.Accessors;
using Umbraco.Cms.Api.Delivery.Configuration;
using Umbraco.Cms.Api.Delivery.Json;
using Umbraco.Cms.Api.Delivery.Rendering;
using Umbraco.Cms.Api.Delivery.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddDeliveryApi(this IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<IRequestStartItemProvider, RequestStartItemProvider>();
        builder.Services.AddScoped<IOutputExpansionStrategy, RequestContextOutputExpansionStrategy>();
        builder.Services.AddSingleton<IRequestCultureService, RequestCultureService>();
        builder.Services.AddSingleton<IRequestRoutingService, RequestRoutingService>();
        builder.Services.AddSingleton<IRequestRedirectService, RequestRedirectService>();
        builder.Services.AddSingleton<IRequestPreviewService, RequestPreviewService>();
        builder.Services.AddSingleton<IOutputExpansionStrategyAccessor, RequestContextOutputExpansionStrategyAccessor>();
        builder.Services.AddSingleton<IRequestStartItemProviderAccessor, RequestContextRequestStartItemProviderAccessor>();
        builder.Services.AddSingleton<IApiAccessService, ApiAccessService>();
        builder.Services.AddSingleton<IApiContentQueryService, ApiContentQueryService>();
        builder.Services.AddSingleton<IApiContentQueryProvider, ApiContentQueryProvider>();

        builder.Services.ConfigureOptions<ConfigureUmbracoDeliveryApiSwaggerGenOptions>();
        builder.AddUmbracoApiOpenApiUI();

        builder
            .Services
            .AddControllers()
            .AddJsonOptions(Constants.JsonOptionsNames.DeliveryApi, options =>
            {
                // all Delivery API specific JSON options go here
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.TypeInfoResolver = new DeliveryApiJsonTypeResolver();
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return builder;
    }
}
