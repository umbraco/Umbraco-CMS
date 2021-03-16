using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Middleware
{
    /// <summary>
    /// Executes when Umbraco booting fails in order to show the problem
    /// </summary>
    public class BootFailedMiddleware : IMiddleware
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BootFailedMiddleware(IRuntimeState runtimeState, IHostingEnvironment hostingEnvironment)
        {
            _runtimeState = runtimeState;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_runtimeState.Level == RuntimeLevel.BootFailed)
            {
                // short circuit
                //

                if (_hostingEnvironment.IsDebugMode)
                {
                    BootFailedException.Rethrow(_runtimeState.BootFailedException);
                }
                else  // Print a nice error page
                {
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
            var fileName = _hostingEnvironment.MapPathWebRoot("~/config/errors/BootFailed.html");
            if (File.Exists(fileName)) return fileName;

            return _hostingEnvironment.MapPathWebRoot("~/umbraco/views/errors/BootFailed.html");
        }
    }
}
