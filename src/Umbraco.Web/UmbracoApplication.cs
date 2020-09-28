using System.Runtime.InteropServices;
using System.Web;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Web.Runtime;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the Umbraco global.asax class.
    /// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
    {
        protected override IRuntime GetRuntime(GlobalSettings globalSettings, ConnectionStrings connectionStrings, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger, ILoggerFactory loggerFactory, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
        {
            var dbProviderFactoryCreator = new UmbracoDbProviderFactoryCreator();


            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = globalSettings.MainDomLock;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock" || isWindows == false
                ? (IMainDomLock)new SqlMainDomLock(loggerFactory.CreateLogger<SqlMainDomLock>(), loggerFactory, globalSettings, connectionStrings, dbProviderFactoryCreator, hostingEnvironment)
                : new MainDomSemaphoreLock(loggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);

            var mainDom = new MainDom(loggerFactory.CreateLogger<MainDom>(), mainDomLock);

            var requestCache = new HttpRequestAppCache(() => HttpContext.Current != null ? HttpContext.Current.Items : null);
            var appCaches = new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                requestCache,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

            var umbracoBootPermissionChecker = new AspNetUmbracoBootPermissionChecker();
            return new CoreRuntime(globalSettings, connectionStrings,umbracoVersion, ioHelper, logger, loggerFactory, profiler, umbracoBootPermissionChecker, hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom,
                GetTypeFinder(hostingEnvironment, logger, profiler), appCaches);
        }


    }
}
