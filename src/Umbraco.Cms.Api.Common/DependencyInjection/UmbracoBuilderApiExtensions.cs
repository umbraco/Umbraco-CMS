using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class UmbracoBuilderApiExtensions
{
    public static IUmbracoBuilder AddUmbracoApiOpenApiUI(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(UmbracoJsonTypeInfoResolver)))
        {
            return builder;
        }

        builder.Services.AddUmbracoApi<ConfigureDefaultApiOptions>(DefaultApiConfiguration.ApiName, "Default API");
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new SwaggerRouteTemplatePipelineFilter("UmbracoApiCommon")));
        builder.Services.AddSingleton<ISchemaIdSelector, SchemaIdSelector>();
        builder.Services.AddSingleton<ISchemaIdHandler, SchemaIdHandler>();

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
            .Configure<IServiceProvider>((swaggerUiOptions, sp) =>
            {
                SwaggerRouteTemplatePipelineFilter? swaggerPipelineFilter = sp.GetRequiredService<IOptions<UmbracoPipelineOptions>>().Value.PipelineFilters.OfType<SwaggerRouteTemplatePipelineFilter>().FirstOrDefault();
                if (swaggerPipelineFilter is null)
                {
                    return;
                }

                var openApiRoute = swaggerPipelineFilter.SwaggerRouteTemplate(sp).Replace("{documentName}", apiName).EnsureStartsWith("/");
                swaggerUiOptions.SwaggerEndpoint(openApiRoute, apiTitle);
                swaggerUiOptions.ConfigObject.Urls = swaggerUiOptions.ConfigObject.Urls.OrderBy(x => x.Name);
            });

        return services;
    }
}
