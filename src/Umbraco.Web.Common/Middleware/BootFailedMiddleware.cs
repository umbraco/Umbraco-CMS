using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
/// Executes when Umbraco booting fails in order to show the problem.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Http.IMiddleware" />
public class BootFailedMiddleware : IMiddleware
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IRuntimeState _runtimeState;

    public BootFailedMiddleware(IRuntimeState runtimeState, IHostingEnvironment hostingEnvironment, IWebHostEnvironment webHostEnvironment)
    {
        _runtimeState = runtimeState;
        _hostingEnvironment = hostingEnvironment;
        _webHostEnvironment = webHostEnvironment;
    }

    [Obsolete("Use ctor with all params. This will be removed in Umbraco 12")]
    public BootFailedMiddleware(IRuntimeState runtimeState, IHostingEnvironment hostingEnvironment)
        : this(runtimeState, hostingEnvironment, StaticServiceProvider.Instance.GetRequiredService<IWebHostEnvironment>())
    { }

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

                IFileInfo? fileInfo = GetBootErrorFileInfo();
                if (fileInfo is not null)
                {
                    using var sr = new StreamReader(fileInfo.CreateReadStream(), Encoding.UTF8);
                    await context.Response.WriteAsync(await sr.ReadToEndAsync(), Encoding.UTF8);
                }
            }
        }
        else
        {
            await next(context);
        }
    }

    private IFileInfo? GetBootErrorFileInfo()
    {
        IFileInfo? fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo("config/errors/BootFailed.html");
        if (fileInfo.Exists)
        {
            return fileInfo;
        }

        fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo("umbraco/views/errors/BootFailed.html");
        if (fileInfo.Exists)
        {
            return fileInfo;
        }

        return null;
    }
}
