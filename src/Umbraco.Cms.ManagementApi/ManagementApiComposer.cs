using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NSwag.AspNetCore;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Configuration;
using Umbraco.Cms.ManagementApi.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.ManagementApi;

public class ManagementApiComposer : IComposer
{
    private const string ApiTitle = "Umbraco Backoffice API";
    private const string ApiAllName = "All";

    private ApiVersion DefaultApiVersion => new(1, 0);

    public void Compose(IUmbracoBuilder builder)
    {
        // TODO Should just call a single extension method that can be called fromUmbracoTestServerTestBase too, instead of calling this method

        IServiceCollection services = builder.Services;

        builder
            .AddNewInstaller()
            .AddUpgrader()
            .AddExamineManagement()
            .AddFactories()
            .AddTrees()
            .AddFactories()
            .AddServices()
            .AddMappers();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = DefaultApiVersion;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.UseApiBehavior = false;
        });

        services.AddOpenApiDocument(options =>
        {
            options.Title = ApiTitle;
            options.Version = ApiAllName;
            options.DocumentName = ApiAllName;
            options.Description = "This shows all APIs available in this version of Umbraco - Including all the legacy apis that is available for backward compatibility";
            options.PostProcess = document =>
            {
                document.Tags = document.Tags.OrderBy(tag => tag.Name).ToList();
            };
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.DefaultApiVersion = DefaultApiVersion;
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.AddApiVersionParametersWhenVersionNeutral = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });
        services.AddControllers();
        builder.Services.ConfigureOptions<ConfigureMvcOptions>();

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                "BackofficeSwagger",
                applicationBuilder =>
                {
                    applicationBuilder.UseExceptionHandler(exceptionBuilder => exceptionBuilder.Run(async context =>
                    {
                        Exception? exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                        if (exception is null)
                        {
                            return;
                        }

                        var response = new ProblemDetails
                        {
                            Title = exception.Message,
                            Detail = exception.StackTrace,
                            Status = StatusCodes.Status500InternalServerError,
                            Instance = exception.GetType().Name,
                            Type = "Error",
                        };
                        await context.Response.WriteAsJsonAsync(response);
                    }));
                },
                applicationBuilder =>
                {
                    IServiceProvider provider = applicationBuilder.ApplicationServices;
                    IWebHostEnvironment webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

                    if (!webHostEnvironment.IsProduction())
                    {
                        GlobalSettings? settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
                        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                        var officePath = settings.GetBackOfficePath(hostingEnvironment);
                        // serve documents (same as app.UseSwagger())
                        applicationBuilder.UseOpenApi(config =>
                        {
                            config.Path = $"{officePath}/swagger/{{documentName}}/swagger.json";
                        });

                        // Serve Swagger UI
                        applicationBuilder.UseSwaggerUi3(config =>
                        {
                            config.Path = officePath + "/swagger";
                            config.SwaggerRoutes.Clear();
                            var swaggerPath = $"{officePath}/swagger/{ApiAllName}/swagger.json";
                            config.SwaggerRoutes.Add(new SwaggerUi3Route(ApiAllName, swaggerPath));
                            config.OperationsSorter = "alpha";
                            config.TagsSorter = "alpha";
                        });
                    }
                },
                applicationBuilder =>
                {
                    IServiceProvider provider = applicationBuilder.ApplicationServices;

                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        GlobalSettings? settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
                        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                        var officePath = settings.GetBackOfficePath(hostingEnvironment);
                        // Maps attribute routed controllers.
                        endpoints.MapControllers();

                        // Serve contract
                        endpoints.MapGet($"{officePath}/api/openapi.json",async  context =>
                        {
                            await context.Response.SendFileAsync(new EmbeddedFileProvider(this.GetType().Assembly).GetFileInfo("OpenApi.json"));
                        });
                    });
                }
            ));
        });
    }
}
