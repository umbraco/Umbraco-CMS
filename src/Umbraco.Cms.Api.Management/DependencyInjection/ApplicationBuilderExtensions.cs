using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseProblemDetailsExceptionHandling(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseWhen(
            httpContext =>
            {
                GlobalSettings settings = httpContext.RequestServices
                    .GetRequiredService<IOptions<GlobalSettings>>().Value;
                IHostingEnvironment hostingEnvironment =
                    httpContext.RequestServices.GetRequiredService<IHostingEnvironment>();
                var officePath = settings.GetBackOfficePath(hostingEnvironment);

                // Only use the API exception handler when we are requesting an API
                // FIXME: magic string "management/api" is used several times across the codebase
                return httpContext.Request.Path.Value?.StartsWith($"{officePath}/management/api/") ?? false;
            },
            innerBuilder =>
            {
                innerBuilder.UseExceptionHandler(exceptionBuilder => exceptionBuilder.Run(async context =>
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
                        Type = "Error"
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }));
            });

    internal static IApplicationBuilder UseSwagger(this IApplicationBuilder applicationBuilder)
    {
        IServiceProvider provider = applicationBuilder.ApplicationServices;
        IWebHostEnvironment webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

        if (webHostEnvironment.IsProduction())
        {
            return applicationBuilder;
        }

        GlobalSettings settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
        var backOfficePath = settings.GetBackOfficePath(hostingEnvironment);

        applicationBuilder.UseSwagger(swaggerOptions =>
        {
            swaggerOptions.RouteTemplate = $"{backOfficePath.TrimStart(Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";
        });
        applicationBuilder.UseSwaggerUI(
            swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint($"{backOfficePath}/swagger/v1/swagger.json", $"{ManagementApiConfiguration.ApiTitle} {ManagementApiConfiguration.DefaultApiVersion}");
                swaggerUiOptions.RoutePrefix = $"{backOfficePath.TrimStart(Constants.CharArrays.ForwardSlash)}/swagger";

                swaggerUiOptions.OAuthClientId(New.Cms.Core.Constants.OauthClientIds.Swagger);
                swaggerUiOptions.OAuthUsePkce();
            });

        return applicationBuilder;
    }

    internal static IApplicationBuilder UseEndpoints(this IApplicationBuilder applicationBuilder)
    {
        IServiceProvider provider = applicationBuilder.ApplicationServices;

        applicationBuilder.UseEndpoints(endpoints =>
        {
            GlobalSettings settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
            IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
            var officePath = settings.GetBackOfficePath(hostingEnvironment);
            // Maps attribute routed controllers.
            endpoints.MapControllers();

            // Serve contract
            // FIXME: magic string "management/api" is used several times across the codebase
            endpoints.MapGet($"{officePath}/management/api/openapi.json", async context =>
            {
                await context.Response.SendFileAsync(new EmbeddedFileProvider(typeof(ManagementApiComposer).Assembly).GetFileInfo("OpenApi.json"));
            });
        });

        return applicationBuilder;
    }
}
