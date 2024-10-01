using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseProblemDetailsExceptionHandling(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseWhen(
            httpContext =>
            {
                var backOfficePath = httpContext.RequestServices.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();

                // Only use the API exception handler when we are requesting an API
                return httpContext.Request.Path.Value?.StartsWith($"{backOfficePath}{Constants.Web.ManagementApiPath}") ?? false;
            },
            innerBuilder =>
            {
                innerBuilder.UseExceptionHandler(exceptionBuilder => exceptionBuilder.Run(async context =>
                {
                    var isDebug = context.RequestServices.GetRequiredService<IHostingEnvironment>().IsDebugMode;
                    Exception? exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                    if (exception is null)
                    {
                        return;
                    }

                    var response = new ProblemDetails
                    {
                        Title = exception.Message,
                        Detail = isDebug ? exception.StackTrace : null,
                        Status = StatusCodes.Status500InternalServerError,
                        Instance = isDebug ? exception.GetType().Name : null,
                        Type = "Error"
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }));
            });

internal static IApplicationBuilder UseEndpoints(this IApplicationBuilder applicationBuilder)
    {
        IServiceProvider provider = applicationBuilder.ApplicationServices;

        applicationBuilder.UseEndpoints(endpoints =>
        {
            var backOfficePath = provider.GetRequiredService<IHostingEnvironment>().GetBackOfficePath();

            // Maps attribute routed controllers.
            endpoints.MapControllers();

            // Serve contract
            endpoints.MapGet($"{backOfficePath}{Constants.Web.ManagementApiPath}openapi.json", async context =>
            {
                await context.Response.SendFileAsync(new EmbeddedFileProvider(typeof(ManagementApiComposer).Assembly).GetFileInfo("OpenApi.json"));
            });
        });

        return applicationBuilder;
    }
}
