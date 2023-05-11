using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class UmbracoBuilderApiExtensions
{
    public static IUmbracoBuilder AddUmbracoApiOpenApiUI(this IUmbracoBuilder builder)
    {
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<ConfigureUmbracoSwaggerGenOptions>();
        builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                "UmbracoApiCommon",
                applicationBuilder =>
                {

                },
                applicationBuilder =>
                {
                    IServiceProvider provider = applicationBuilder.ApplicationServices;
                    IWebHostEnvironment webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
                    IOptions<SwaggerGenOptions> swaggerGenOptions = provider.GetRequiredService<IOptions<SwaggerGenOptions>>();


                    if (!webHostEnvironment.IsProduction())
                    {
                        GlobalSettings? settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
                        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                        var umbracoPath = settings.GetBackOfficePath(hostingEnvironment);

                        applicationBuilder.UseSwagger(swaggerOptions =>
                        {
                            swaggerOptions.RouteTemplate =
                                $"{umbracoPath.TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";
                        });
                        applicationBuilder.UseSwaggerUI(
                            swaggerUiOptions =>
                            {
                                swaggerUiOptions.RoutePrefix = $"{umbracoPath.TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

                                foreach ((var name, OpenApiInfo? apiInfo) in swaggerGenOptions.Value.SwaggerGeneratorOptions.SwaggerDocs.OrderBy(x=>x.Value.Title))
                                {
                                    swaggerUiOptions.SwaggerEndpoint($"{name}/swagger.json", $"{apiInfo.Title}");
                                }
                            });
                    }
                },
                applicationBuilder =>
                {

                }));
        });

        return builder;
    }
}
