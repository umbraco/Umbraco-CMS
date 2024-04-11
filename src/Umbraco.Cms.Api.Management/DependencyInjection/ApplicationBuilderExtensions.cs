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
                IHostingEnvironment hostingEnvironment = httpContext.RequestServices.GetRequiredService<IHostingEnvironment>();
                var backOfficePath = hostingEnvironment.ToAbsolute(Constants.System.DefaultUmbracoPath);

                // Only use the API exception handler when we are requesting an API
                return httpContext.Request.Path.Value?.StartsWith($"{backOfficePath}{Constants.Web.ManagementApiPath}") ?? false;
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

    internal static IApplicationBuilder UseEndpoints(this IApplicationBuilder applicationBuilder)
    {
        IServiceProvider provider = applicationBuilder.ApplicationServices;

        applicationBuilder.UseEndpoints(endpoints =>
        {
            IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
            var backOfficePath = hostingEnvironment.ToAbsolute(Constants.System.DefaultUmbracoPath);

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
