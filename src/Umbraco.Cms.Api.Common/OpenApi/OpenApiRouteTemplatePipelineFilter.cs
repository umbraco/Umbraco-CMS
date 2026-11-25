using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Common.OpenApi;

internal class OpenApiRouteTemplatePipelineFilter : UmbracoPipelineFilter
{
    public OpenApiRouteTemplatePipelineFilter(string name)
        : base(name)
    {
        PostPipeline = PostPipelineAction;
        PreMapEndpoints = OnPreMapEndpointsAction;
    }

    private static void PostPipelineAction(IApplicationBuilder applicationBuilder)
        => GetUiProvider(applicationBuilder.ApplicationServices)?.UseOpenApiUi(applicationBuilder);

    private static void OnPreMapEndpointsAction(IEndpointRouteBuilder endpoints)
    {
        UmbracoOpenApiOptions options = endpoints.ServiceProvider
            .GetRequiredService<IOptions<UmbracoOpenApiOptions>>().Value;

        if (options.Enabled is false)
        {
            return;
        }

        endpoints.MapOpenApi(options.RouteTemplate);
        GetUiProvider(endpoints.ServiceProvider)?.MapOpenApiUi(endpoints);
    }

    private static IOpenApiUiProvider? GetUiProvider(IServiceProvider serviceProvider)
    {
        UmbracoOpenApiOptions options = serviceProvider.GetRequiredService<IOptions<UmbracoOpenApiOptions>>().Value;

        return options.Enabled && options.DefaultUiEnabled
            ? serviceProvider.GetService<IOpenApiUiProvider>()
            : null;
    }
}
