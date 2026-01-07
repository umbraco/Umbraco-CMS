using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class OpenApiRouteTemplatePipelineFilter : UmbracoPipelineFilter
{
    public OpenApiRouteTemplatePipelineFilter(string name)
        : base(name)
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
    {
        UmbracoOpenApiOptions options = applicationBuilder.ApplicationServices
            .GetRequiredService<IOptions<UmbracoOpenApiOptions>>().Value;

        if (options.Enabled is false)
        {
            return;
        }

        // TODO: Check if there is a better way to do this without calling UseEndpoints twice
        applicationBuilder.UseEndpoints(e => e.MapOpenApi(options.RouteTemplate));
        applicationBuilder.UseSwaggerUI(swaggerUiOptions => ConfigureSwaggerUi(swaggerUiOptions, options));
    }

    private void ConfigureSwaggerUi(SwaggerUIOptions swaggerUiOptions, UmbracoOpenApiOptions options)
    {
        swaggerUiOptions.RoutePrefix = options.UiRoutePrefix;

        // Add custom configuration from https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        swaggerUiOptions.ConfigObject.PersistAuthorization = true; // persists authorization data so it would not be lost on browser close/refresh
        swaggerUiOptions.ConfigObject.Filter = string.Empty; // Enable the filter with an empty string as default filter.

        swaggerUiOptions.OAuthClientId(Constants.OAuthClientIds.Swagger);
        swaggerUiOptions.OAuthUsePkce();
    }
}
