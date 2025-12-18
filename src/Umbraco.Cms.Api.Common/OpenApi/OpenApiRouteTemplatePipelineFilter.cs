using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class OpenApiRouteTemplatePipelineFilter : UmbracoPipelineFilter
{
    public OpenApiRouteTemplatePipelineFilter(string name)
        : base(name)
    {
        PostPipeline = PostPipelineAction;
        PreMapEndpoints = OnPreMapEndpointsAction;
    }

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
    {
        if (OpenApiIsEnabled(applicationBuilder.ApplicationServices) is false)
        {
            return;
        }

        applicationBuilder.UseSwaggerUI(swaggerUiOptions => ConfigureOpenApiUI(swaggerUiOptions, applicationBuilder.ApplicationServices));
    }

    private void OnPreMapEndpointsAction(IEndpointRouteBuilder endpoints)
    {
        if (OpenApiIsEnabled(endpoints.ServiceProvider) is false)
        {
            return;
        }

        endpoints.MapOpenApi(OpenApiRouteTemplate(endpoints.ServiceProvider));
    }

    protected virtual bool OpenApiIsEnabled(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IWebHostEnvironment>().IsProduction() is false;

    public virtual string OpenApiRouteTemplate(IServiceProvider serviceProvider)
            => $"{GetBackOfficePath(serviceProvider).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";

    public virtual string OpenApiUiRoutePrefix(IServiceProvider serviceProvider)
        => $"{GetBackOfficePath(serviceProvider).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

    protected virtual void ConfigureOpenApiUI(
        SwaggerUIOptions swaggerUiOptions,
        IServiceProvider serviceProvider)
    {
        swaggerUiOptions.RoutePrefix = OpenApiUiRoutePrefix(serviceProvider);

        // Add custom configuration from https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        swaggerUiOptions.ConfigObject.PersistAuthorization = true; // persists authorization data so it would not be lost on browser close/refresh
        swaggerUiOptions.ConfigObject.Filter = string.Empty; // Enable the filter with an empty string as default filter.

        swaggerUiOptions.OAuthClientId(Constants.OAuthClientIds.Swagger);
        swaggerUiOptions.OAuthUsePkce();
    }

    private static string GetBackOfficePath(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();
}
