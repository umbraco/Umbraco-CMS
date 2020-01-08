using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Runtime;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the Umbraco global.asax class.
    /// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
    {
        protected override IRuntime GetRuntime(Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
        {

            var connectionStringConfig = configs.ConnectionStrings()[Constants.System.UmbracoConnectionName];

            var dbProviderFactoryCreator = new UmbracoDbProviderFactoryCreator(connectionStringConfig.ProviderName);
            var bulkSqlInsertProvider = connectionStringConfig.ProviderName == Constants.DbProviderNames.SqlCe ? (IBulkSqlInsertProvider) new SqlCeBulkSqlInsertProvider() : new SqlServerBulkSqlInsertProvider();
            var mainDom = new MainDom(logger, hostingEnvironment);

            return new WebRuntime(this, configs, umbracoVersion, ioHelper, logger, profiler, hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, bulkSqlInsertProvider, mainDom);
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
