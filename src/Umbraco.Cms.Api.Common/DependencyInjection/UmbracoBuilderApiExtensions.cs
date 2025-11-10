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
        if (builder.Services.Any(x => !x.IsKeyedService && x.ImplementationType == typeof(UmbracoJsonTypeInfoResolver)))
        {
            return builder;
        }

        builder.Services.AddOpenApi(DefaultApiConfiguration.ApiName);
        builder.Services.ConfigureOptions<ConfigureDefaultApiOptions>();
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
        builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new SwaggerRouteTemplatePipelineFilter("UmbracoApiCommon")));

        return builder;
    }
}
