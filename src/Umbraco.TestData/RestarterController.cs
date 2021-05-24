using System.Web.Mvc;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using System.IO;
using System.Web;
using System.Threading;
using Umbraco.Core.Configuration;
using System;

namespace Umbraco.TestData
{
    public class RestarterController : SurfaceController
    {
        private readonly IGlobalSettings _globalSettings;

        public RestarterController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger, umbracoHelper)
        {
            _globalSettings = globalSettings;
        }

        public ActionResult GCCollect()
        {
            GC.Collect(0, GCCollectionMode.Forced, true);

            return Content("GC Collected");
        }

        /// <summary>
        /// Causes a warmboot restart
        /// </summary>
        /// <returns></returns>
        public ActionResult WarmBoot()
        {
            HttpContext.User = null;
            System.Web.HttpContext.Current.User = null;
            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();

            return Content("warm rebooting");
        }

        /// <summary>
        /// Causes a coldboot restart
        /// </summary>
        /// <returns></returns>
        public ActionResult ColdBoot()
        {
            // Delete the DistCache
            var distCacheFolder = Path.Combine(_globalSettings.LocalTempPath, "DistCache");
            Directory.Delete(distCacheFolder, true);

            HttpContext.User = null;
            System.Web.HttpContext.Current.User = null;
            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();

            return Content("cold rebooting");
        }

    }
}
