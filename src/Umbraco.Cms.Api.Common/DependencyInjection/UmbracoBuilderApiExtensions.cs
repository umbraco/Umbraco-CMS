using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder"/> to configure API services.
/// </summary>
public static class UmbracoBuilderApiExtensions
{
    /// <summary>
    ///     Adds Umbraco API OpenAPI/Swagger UI services to the builder.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    internal static void AddUmbracoOpenApi(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(UmbracoJsonTypeInfoResolver)))
        {
            return;
        }

        builder.Services.AddOptions<UmbracoOpenApiOptions>()
            .Configure<IHostingEnvironment, IWebHostEnvironment>((options, hostingEnv, webHostEnv) =>
            {
                options.Enabled = webHostEnv.IsProduction() is false;
                var backOfficePath = hostingEnv.GetBackOfficePath().TrimStart(Constants.CharArrays.ForwardSlash);
                options.RouteTemplate = $"{backOfficePath}/openapi/{{documentName}}.json";
                options.UiRoutePrefix = $"{backOfficePath}/openapi";
            });
        builder.AddUmbracoOpenApiDocument<ConfigureDefaultApiOptions>(DefaultApiConfiguration.ApiName, "Default API");
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new OpenApiRouteTemplatePipelineFilter("UmbracoApiCommon")));
    }

    /// <summary>
    /// Adds and configures an Umbraco OpenAPI document with shared transformers.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="apiName">The name/identifier of the API.</param>
    /// <param name="apiTitle">The title of the API.</param>
    /// <param name="jsonOptionsName">
    /// Optional named <c>JsonOptions</c> to use for schema generation instead of the default HTTP JSON options.
    /// When specified, replaces the internal <c>OpenApiSchemaService</c> registration for this document.
    /// </param>
    /// <typeparam name="TConfigureOptions">The type used to configure the OpenAPI options.</typeparam>
    internal static void AddUmbracoOpenApiDocument<TConfigureOptions>(
        this IUmbracoBuilder builder,
        string apiName,
        string apiTitle,
        string? jsonOptionsName = null)
        where TConfigureOptions : ConfigureUmbracoOpenApiOptionsBase
    {
        builder.Services.AddOpenApi(apiName);
        builder.Services.ConfigureOptions<TConfigureOptions>();
        builder.Services.AddOpenApiDocumentToUi(apiName, apiTitle);

        if (jsonOptionsName is not null)
        {
            builder.Services.ReplaceOpenApiSchemaService(apiName, jsonOptionsName);
        }
    }
}
