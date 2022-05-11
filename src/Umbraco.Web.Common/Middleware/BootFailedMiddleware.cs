using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
///     Executes when Umbraco booting fails in order to show the problem
/// </summary>
public class BootFailedMiddleware : IMiddleware
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRuntimeState _runtimeState;

    public BootFailedMiddleware(IRuntimeState runtimeState, IHostingEnvironment hostingEnvironment)
    : this(runtimeState, hostingEnvironment, StaticServiceProvider.Instance.GetRequiredService<IWebHostEnvironment>())
    {
        _runtimeState = runtimeState;
        _hostingEnvironment = hostingEnvironment;
    }

    public BootFailedMiddleware(IRuntimeState runtimeState, IHostingEnvironment hostingEnvironment, IWebHostEnvironment webHostEnvironment)
    {
        _runtimeState = runtimeState;
        _hostingEnvironment = hostingEnvironment;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // TODO: It would be possible to redirect to the installer here in debug mode while
        // still showing the error. This would be a lot more friendly than just the YSOD.
        // We could also then have a different installer view for when package migrations fails
        // and to retry each one individually. Perhaps this can happen in the future.
        if (_runtimeState.Level == RuntimeLevel.BootFailed)
        {
            // short circuit
            if (_hostingEnvironment.IsDebugMode)
            {
                BootFailedException.Rethrow(_runtimeState.BootFailedException);
            }
            else
            {
                // Print a nice error page
                context.Response.Clear();
                context.Response.StatusCode = 500;

                var file = GetBootErrorFileName();

                var viewContent = await File.ReadAllTextAsync(file);
                await context.Response.WriteAsync(viewContent, Encoding.UTF8);
            }
        }
        else
        {
            await next(context);
        }
    }

    private string GetBootErrorFileName()
    {
        var fileName = _webHostEnvironment.MapPathWebRoot("~/config/errors/BootFailed.html");
        if (File.Exists(fileName))
        {
            return fileName;
        }

        return _webHostEnvironment.MapPathWebRoot("~/umbraco/views/errors/BootFailed.html");
    }
}
