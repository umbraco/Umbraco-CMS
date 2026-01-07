using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class UmbracoBuilderApiExtensions
{
    public static IUmbracoBuilder AddUmbracoApiOpenApiUI(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(UmbracoJsonTypeInfoResolver)))
        {
            return builder;
        }

        builder.Services.AddOptions<UmbracoOpenApiOptions>()
            .Configure<IHostingEnvironment, IWebHostEnvironment>((options, hostingEnv, webHostEnv) =>
            {
                options.Enabled = webHostEnv.IsProduction() is false;
                var backOfficePath = hostingEnv.GetBackOfficePath().TrimStart(Constants.CharArrays.ForwardSlash);
                options.RouteTemplate = $"{backOfficePath}/openapi/{{documentName}}.json";
                options.UiRoutePrefix = $"{backOfficePath}/openapi";
            });
        builder.Services.AddUmbracoApi<ConfigureDefaultApiOptions>(DefaultApiConfiguration.ApiName, "Default API");
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new OpenApiRouteTemplatePipelineFilter("UmbracoApiCommon")));

        return builder;
    }

    /// <summary>
    /// Adds and configures an Umbraco API with OpenAPI documentation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="apiName">The name/identifier of the API.</param>
    /// <param name="apiTitle">The title of the API.</param>
    /// <typeparam name="TConfigureOptions">The type used to configure the OpenAPI options.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddUmbracoApi<TConfigureOptions>(
        this IServiceCollection services,
        string apiName,
        string apiTitle)
        where TConfigureOptions : ConfigureUmbracoOpenApiOptionsBase
    {
        services.AddOpenApi(apiName);
        services.ConfigureOptions<TConfigureOptions>();
        services.AddOptions<SwaggerUIOptions>()
            .Configure<IOptions<UmbracoOpenApiOptions>>((swaggerUiOptions, openApiOptions) =>
            {
                var openApiRoute = openApiOptions.Value.RouteTemplate.Replace("{documentName}", apiName).EnsureStartsWith("/");
                swaggerUiOptions.SwaggerEndpoint(openApiRoute, apiTitle);
                swaggerUiOptions.ConfigObject.Urls = swaggerUiOptions.ConfigObject.Urls.OrderBy(x => x.Name);
            });

        return services;
    }
}
