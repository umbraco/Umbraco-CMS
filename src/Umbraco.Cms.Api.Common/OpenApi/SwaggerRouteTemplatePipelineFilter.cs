using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
///     Pipeline filter that configures Swagger/OpenAPI endpoints for Umbraco APIs.
/// </summary>
public class SwaggerRouteTemplatePipelineFilter : UmbracoPipelineFilter
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SwaggerRouteTemplatePipelineFilter"/> class.
    /// </summary>
    /// <param name="name">The name of the pipeline filter.</param>
    public SwaggerRouteTemplatePipelineFilter(string name)
        : base(name)
        => PostPipeline = PostPipelineAction;

    private void PostPipelineAction(IApplicationBuilder applicationBuilder)
    {
        if (SwaggerIsEnabled(applicationBuilder) is false)
        {
            return;
        }

        IOptions<SwaggerGenOptions> swaggerGenOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();

        applicationBuilder.UseSwagger(swaggerOptions =>
        {
            swaggerOptions.RouteTemplate = SwaggerRouteTemplate(applicationBuilder);
        });

        applicationBuilder.UseSwaggerUI(swaggerUiOptions => SwaggerUiConfiguration(swaggerUiOptions, swaggerGenOptions.Value, applicationBuilder));
    }

    /// <summary>
    ///     Determines whether Swagger is enabled for the application.
    /// </summary>
    /// <param name="applicationBuilder">The application builder.</param>
    /// <returns><c>true</c> if Swagger is enabled; otherwise, <c>false</c>.</returns>
    protected virtual bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder)
        => applicationBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction() is false;

    /// <summary>
    ///     Gets the route template for Swagger JSON endpoints.
    /// </summary>
    /// <param name="applicationBuilder">The application builder.</param>
    /// <returns>The Swagger route template.</returns>
    protected virtual string SwaggerRouteTemplate(IApplicationBuilder applicationBuilder)
        => $"{GetBackOfficePath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";

    /// <summary>
    ///     Gets the route prefix for the Swagger UI.
    /// </summary>
    /// <param name="applicationBuilder">The application builder.</param>
    /// <returns>The Swagger UI route prefix.</returns>
    protected virtual string SwaggerUiRoutePrefix(IApplicationBuilder applicationBuilder)
        => $"{GetBackOfficePath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

    /// <summary>
    ///     Configures the Swagger UI options.
    /// </summary>
    /// <param name="swaggerUiOptions">The Swagger UI options to configure.</param>
    /// <param name="swaggerGenOptions">The Swagger generation options.</param>
    /// <param name="applicationBuilder">The application builder.</param>
    protected virtual void SwaggerUiConfiguration(
        SwaggerUIOptions swaggerUiOptions,
        SwaggerGenOptions swaggerGenOptions,
        IApplicationBuilder applicationBuilder)
    {
        swaggerUiOptions.RoutePrefix = SwaggerUiRoutePrefix(applicationBuilder);

        foreach ((var name, OpenApiInfo? apiInfo) in swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.OrderBy(x => x.Value.Title))
        {
            swaggerUiOptions.SwaggerEndpoint($"{name}/swagger.json", $"{apiInfo.Title}");
        }

        // Add custom configuration from https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/
        swaggerUiOptions.ConfigObject.PersistAuthorization = true; // persists authorization data so it would not be lost on browser close/refresh
        swaggerUiOptions.ConfigObject.Filter = string.Empty; // Enable the filter with an empty string as default filter.

        swaggerUiOptions.OAuthClientId(Constants.OAuthClientIds.Swagger);
        swaggerUiOptions.OAuthUsePkce();
    }

    private string GetBackOfficePath(IApplicationBuilder applicationBuilder)
        => applicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();
}
