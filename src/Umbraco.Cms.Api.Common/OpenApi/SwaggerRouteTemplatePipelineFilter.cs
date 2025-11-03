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

public class SwaggerRouteTemplatePipelineFilter : UmbracoPipelineFilter
{
    public SwaggerRouteTemplatePipelineFilter(string name)
        : base(name)
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
    {
        if (SwaggerIsEnabled(applicationBuilder) is false)
        {
            return;
        }

        // TODO: Check whether to register the open api generation here or in each specific project
        // IOptions<SwaggerGenOptions> swaggerGenOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();
        //
        // applicationBuilder.UseSwagger(swaggerOptions =>
        // {
        //     swaggerOptions.RouteTemplate = SwaggerRouteTemplate(applicationBuilder);
        // });

        applicationBuilder.UseSwaggerUI(swaggerUiOptions => SwaggerUiConfiguration(swaggerUiOptions, applicationBuilder));
    }

    protected virtual bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder)
        => applicationBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction() is false;

    protected virtual string SwaggerRouteTemplate(IApplicationBuilder applicationBuilder)
        => $"{GetBackOfficePath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";

    protected virtual string SwaggerUiRoutePrefix(IApplicationBuilder applicationBuilder)
        => $"{GetBackOfficePath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

    protected virtual void SwaggerUiConfiguration(
        SwaggerUIOptions swaggerUiOptions,
        IApplicationBuilder applicationBuilder)
    {
        swaggerUiOptions.RoutePrefix = SwaggerUiRoutePrefix(applicationBuilder);

        // TODO: Move this to each specific API project so that each API can define its own info
        swaggerUiOptions.SwaggerEndpoint("/umbraco/swagger/default/swagger.json", "Default");
        swaggerUiOptions.SwaggerEndpoint("/umbraco/swagger/delivery/swagger.json", "Delivery");
        swaggerUiOptions.SwaggerEndpoint("/umbraco/swagger/management/swagger.json", "Management");

        // Add custom configuration from https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        swaggerUiOptions.ConfigObject.PersistAuthorization = true; // persists authorization data so it would not be lost on browser close/refresh
        swaggerUiOptions.ConfigObject.Filter = string.Empty; // Enable the filter with an empty string as default filter.

        swaggerUiOptions.OAuthClientId(Constants.OAuthClientIds.Swagger);
        swaggerUiOptions.OAuthUsePkce();
    }

    private string GetBackOfficePath(IApplicationBuilder applicationBuilder)
        => applicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();
}
