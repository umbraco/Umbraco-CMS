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
        if (SwaggerIsEnabled(applicationBuilder) is false)
        {
            return;
        }

        // TODO: Check if there is a better way to do this without calling UseEndpoints twice
        applicationBuilder.UseEndpoints(e => e.MapOpenApi(SwaggerRouteTemplate(applicationBuilder)));
        applicationBuilder.UseSwaggerUI(swaggerUiOptions => SwaggerUiConfiguration(swaggerUiOptions, applicationBuilder));
    }

    protected virtual bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder)
        => applicationBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction() is false;

    public virtual string SwaggerRouteTemplate(IServiceProvider serviceProvider)
            => $"{GetBackOfficePath(serviceProvider).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";

    [Obsolete("Use the non-obsolete overload that accepts IServiceProvider instead. Scheduled for removal in v19.")]
    protected virtual string SwaggerRouteTemplate(IApplicationBuilder applicationBuilder)
        => SwaggerRouteTemplate(applicationBuilder.ApplicationServices);

    public virtual string SwaggerUiRoutePrefix(IServiceProvider serviceProvider)
        => $"{GetBackOfficePath(serviceProvider).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

    [Obsolete("Use the non-obsolete overload that accepts IServiceProvider instead. Scheduled for removal in v19.")]
    protected virtual string SwaggerUiRoutePrefix(IApplicationBuilder applicationBuilder)
        => SwaggerUiRoutePrefix(applicationBuilder.ApplicationServices);

    protected virtual void SwaggerUiConfiguration(
        SwaggerUIOptions swaggerUiOptions,
        IApplicationBuilder applicationBuilder)
    {
        swaggerUiOptions.RoutePrefix = SwaggerUiRoutePrefix(applicationBuilder);

        // Add custom configuration from https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        swaggerUiOptions.ConfigObject.PersistAuthorization = true; // persists authorization data so it would not be lost on browser close/refresh
        swaggerUiOptions.ConfigObject.Filter = string.Empty; // Enable the filter with an empty string as default filter.

        swaggerUiOptions.OAuthClientId(Constants.OAuthClientIds.Swagger);
        swaggerUiOptions.OAuthUsePkce();
    }

    private string GetBackOfficePath(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();
}
