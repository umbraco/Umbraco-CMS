using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

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
    /// <returns>The Umbraco builder for method chaining.</returns>
    public static IUmbracoBuilder AddUmbracoApiOpenApiUI(this IUmbracoBuilder builder)
    {
        if (builder.Services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(OperationIdSelector)))
        {
            return builder;
        }

        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<ConfigureUmbracoSwaggerGenOptions>();
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
        builder.Services.AddSingleton<IOperationIdSelector, OperationIdSelector>();
        builder.Services.AddSingleton<IOperationIdHandler, OperationIdHandler>();
        builder.Services.AddSingleton<ISchemaIdSelector, SchemaIdSelector>();
        builder.Services.AddSingleton<ISchemaIdHandler, SchemaIdHandler>();
        builder.Services.AddSingleton<ISubTypesSelector, SubTypesSelector>();
        builder.Services.AddSingleton<ISubTypesHandler, SubTypesHandler>();
        builder.Services.AddSingleton<IDocumentInclusionSelector, DocumentInclusionSelector>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new SwaggerRouteTemplatePipelineFilter("UmbracoApiCommon")));

        return builder;
    }
}
