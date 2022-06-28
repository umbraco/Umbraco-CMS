using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.BackOfficeApi;

public class NewBackofficeComposer : IComposer
{
    private const string ApiTitle = "Umbraco Backoffice API";
    private const string ApiDefaultName = "Current";
    private const string ApiAllName = "All";


    public void Compose(IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

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
                }
            ));
        });
    }
}
