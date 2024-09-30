using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class UmbracoBuilderApiExtensions
{
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
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new SwaggerRouteTemplatePipelineFilter("UmbracoApiCommon")));

        return builder;
    }
}
