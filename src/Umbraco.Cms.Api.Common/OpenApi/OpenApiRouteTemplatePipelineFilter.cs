using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
    {
        if (OpenApiIsEnabled(applicationBuilder.ApplicationServices) is false)
        {
            return;
        }

        // TODO: Check if there is a better way to do this without calling UseEndpoints twice
        applicationBuilder.UseEndpoints(e => e.MapOpenApi(OpenApiRouteTemplate(e.ServiceProvider)));
        applicationBuilder.UseSwaggerUI(swaggerUiOptions => ConfigureOpenApiUI(swaggerUiOptions, applicationBuilder.ApplicationServices));
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
