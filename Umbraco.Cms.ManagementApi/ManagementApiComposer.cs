using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi;

public class ManagementApiComposer : IComposer
{
    private const string ApiTitle = "Umbraco Backoffice API";
    private const string ApiAllName = "All";


    public void Compose(IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        builder.AddNewInstaller();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
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
        });

        // Not super happy with this, but we need to know the UmbracoPath when registering the controller
        // To be able to replace the route template token
        GlobalSettings? globalSettings =
            builder.Config.GetSection(Constants.Configuration.ConfigGlobal).Get<GlobalSettings>();
        var backofficePath = globalSettings.UmbracoPath.TrimStart(Constants.CharArrays.TildeForwardSlash);

        services.AddControllers(options =>
        {
            options.Conventions.Add(new UmbracoBackofficeToken("backoffice", backofficePath));
        });

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
                    applicationBuilder.UseOpenApi();  // serve documents (same as app.UseSwagger())
                    applicationBuilder.UseSwaggerUi3(); // Serve Swagger UI
                },
                applicationBuilder =>
                {
                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        // Maps attribute routed controllers.
                        endpoints.MapControllers();
                    });
                }
            ));
        });
    }
}
