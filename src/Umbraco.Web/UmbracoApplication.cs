using System.Runtime.InteropServices;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Infrastructure.Configuration;
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

            var dbProviderFactoryCreator = new UmbracoDbProviderFactoryCreator();

            var globalSettings = ConfigModelConversionsFromLegacy.ConvertGlobalSettings(configs.Global());
            var connectionStrings = ConfigModelConversionsFromLegacy.ConvertConnectionStrings(configs.ConnectionStrings());

            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = globalSettings.MainDomLock;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock" || isWindows == false
                ? (IMainDomLock)new SqlMainDomLock(logger, globalSettings, connectionStrings, dbProviderFactoryCreator, hostingEnvironment)
                : new MainDomSemaphoreLock(logger, hostingEnvironment);

            var mainDom = new MainDom(logger, mainDomLock);

            var requestCache = new HttpRequestAppCache(() => HttpContext.Current != null ? HttpContext.Current.Items : null);
            var umbracoBootPermissionChecker = new AspNetUmbracoBootPermissionChecker();
            return new CoreRuntime(globalSettings, connectionStrings, umbracoVersion, ioHelper, logger, profiler, umbracoBootPermissionChecker, hostingEnvironment, backOfficeInfo, dbProviderFactoryCreator, mainDom,
                GetTypeFinder(hostingEnvironment, logger, profiler), requestCache);
        }


    }
}
