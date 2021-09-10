using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Runtime;
using Umbraco.Web.Runtime;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the Umbraco global.asax class.
    /// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
    {
        protected override IRuntime GetRuntime()
        {
            var logger = SerilogLogger.CreateWithDefaultConfiguration();

            var runtime = new WebRuntime(this, logger, GetMainDom(logger));

            return runtime;
        }

        protected override void OnApplicationError(object sender, EventArgs evargs)
        {
            base.OnApplicationError(sender, evargs);

            // if the exception is a BootFailedException we want to show a custom 500 page
            if (Server.GetLastError() is BootFailedException)
            {
                // if the requested file exists on disk, clear the error and return
                // this is needed to serve static files
                if (File.Exists(Request.PhysicalPath))
                {
                    Server.ClearError();
                    return;
                }

                // if the application is in debug mode we don't want to show the custom 500 page
                if (Context.IsDebuggingEnabled) return;

                // find the error file to show
                var fileName = GetBootErrorFileName();

                // if the file doesn't exist we return and a YSOD will be shown
                if (File.Exists(fileName) == false) return;

                Response.TrySkipIisCustomErrors = true;
                Server.ClearError();
                Response.Clear();

                Response.StatusCode = 500;
                Response.ContentType = "text/html";
                Response.WriteFile(fileName);

                CompleteRequest();
            }
        }

        /// <summary>
        /// Returns the absolute filename to the BootException html file.
        /// </summary>
        protected virtual string GetBootErrorFileName()
        {
            var fileName = Server.MapPath("~/config/errors/BootFailed.html");
            if (File.Exists(fileName)) return fileName;

            return Server.MapPath("~/umbraco/views/errors/BootFailed.html");
        }

        /// <summary>
        /// Returns a new MainDom
        /// </summary>
        protected virtual IMainDom GetMainDom(ILogger logger)
        {
            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = ConfigurationManager.AppSettings[Constants.AppSettings.MainDomLock];

            // TODO: Can we automatically and consistently determine we're running on Azure without this app setting?

            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock"
                ? (IMainDomLock)new SqlMainDomLock(logger)
                : new MainDomSemaphoreLock(logger);

            return new MainDom(logger, mainDomLock);
        }

        /// <summary>
        /// Restarts the Umbraco application.
        /// </summary>
        public static void Restart()
        {
            // see notes in overload

            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Application.Add("AppPoolRestarting", true);
                httpContext.User = null;
            }
            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();
        }

        /// <summary>
        /// Restarts the Umbraco application.
        /// </summary>
        public static void Restart(HttpContextBase httpContext)
        {
            if (httpContext != null)
            {
                // we're going to put an application wide flag to show that the application is about to restart.
                // we're doing this because if there is a script checking if the app pool is fully restarted, then
                // it can check if this flag exists...  if it does it means the app pool isn't restarted yet.
                httpContext.Application.Add("AppPoolRestarting", true);

                // unload app domain - we must null out all identities otherwise we get serialization errors
                // http://www.zpqrtbnk.net/posts/custom-iidentity-serialization-issue
                httpContext.User = null;
            }

            if (HttpContext.Current != null)
                HttpContext.Current.User = null;

            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();
        }
    }
}
