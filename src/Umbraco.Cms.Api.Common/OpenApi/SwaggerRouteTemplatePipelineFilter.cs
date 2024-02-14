using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
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

        IOptions<SwaggerGenOptions> swaggerGenOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<SwaggerGenOptions>>();

        applicationBuilder.UseSwagger(swaggerOptions =>
            {
                swaggerOptions.RouteTemplate = SwaggerRouteTemplate(applicationBuilder);
            });

        applicationBuilder.UseSwaggerUI(swaggerUiOptions => SwaggerUiConfiguration(swaggerUiOptions, swaggerGenOptions.Value, applicationBuilder));
    }

    protected virtual bool SwaggerIsEnabled(IApplicationBuilder applicationBuilder)
    {
        IWebHostEnvironment webHostEnvironment = applicationBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        return webHostEnvironment.IsProduction() is false;
    }

    protected virtual string SwaggerRouteTemplate(IApplicationBuilder applicationBuilder)
        => $"{GetUmbracoPath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";

    protected virtual string SwaggerUiRoutePrefix(IApplicationBuilder applicationBuilder)
        => $"{GetUmbracoPath(applicationBuilder).TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

    protected virtual void SwaggerUiConfiguration(
        SwaggerUIOptions swaggerUiOptions,
        SwaggerGenOptions swaggerGenOptions,
        IApplicationBuilder applicationBuilder)
    {
        swaggerUiOptions.RoutePrefix = SwaggerUiRoutePrefix(applicationBuilder);

        foreach ((var name, OpenApiInfo? apiInfo) in swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs
                     .OrderBy(x => x.Value.Title))
        {
            swaggerUiOptions.SwaggerEndpoint($"{name}/swagger.json", $"{apiInfo.Title}");
        }

        swaggerUiOptions.OAuthUsePkce();
    }

    private string GetUmbracoPath(IApplicationBuilder applicationBuilder)
    {
        GlobalSettings settings = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
        IHostingEnvironment hostingEnvironment = applicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>();

        return settings.GetBackOfficePath(hostingEnvironment);
    }
}
