using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Extensions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Install;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Install
{
    /// <summary>
    /// The MVC Installation controller
    /// </summary>
    /// <remarks>
    /// NOTE: All views must have their full paths as we do not have a custom view engine for the installation views!
    /// </remarks>
    [InstallAuthorize]
    public class InstallController : Controller
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly InstallHelper _installHelper;
        private readonly IRuntimeState _runtime;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public InstallController(
            IUmbracoContextAccessor umbracoContextAccessor,
            InstallHelper installHelper,
            IRuntimeState runtime,
            IGlobalSettings globalSettings,
            IRuntimeMinifier runtimeMinifier,
            IHostingEnvironment hostingEnvironment,
            IUmbracoVersion umbracoVersion)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _installHelper = installHelper;
            _runtime = runtime;
            _globalSettings = globalSettings;
            _runtimeMinifier = runtimeMinifier;
            _hostingEnvironment = hostingEnvironment;
            _umbracoVersion = umbracoVersion;
        }

        [HttpGet]
        [StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable)]
        [TypeFilter(typeof(StatusCodeResultAttribute), Arguments = new object []{System.Net.HttpStatusCode.ServiceUnavailable})]
        public ActionResult Index()
        {
            if (_runtime.Level == RuntimeLevel.Run)
                return Redirect(_globalSettings.UmbracoPath.EnsureEndsWith('/'));

            if (_runtime.Level == RuntimeLevel.Upgrade)
            {
                // Update ClientDependency version and delete its temp directories to make sure we get fresh caches
                _runtimeMinifier.Reset();

                var result = _umbracoContextAccessor.UmbracoContext.Security.ValidateCurrentUser(false);

                switch (result)
                {
                    case ValidateRequestAttempt.FailedNoPrivileges:
                    case ValidateRequestAttempt.FailedNoContextId:
                        return Redirect(_globalSettings.UmbracoPath + "/AuthorizeUpgrade?redir=" + Request.GetEncodedUrl());
                }
            }

            // gen the install base urlAddUmbracoCore
            ViewData.SetInstallApiBaseUrl(Url.GetUmbracoApiService("GetSetup", "InstallApi", "UmbracoInstall").TrimEnd("GetSetup"));

            // get the base umbraco folder
            ViewData.SetUmbracoBaseFolder(_hostingEnvironment.ToAbsolute(_globalSettings.UmbracoPath));

            ViewData.SetUmbracoVersion(_umbracoVersion.SemanticVersion);

            _installHelper.InstallStatus(false, "");

            // always ensure full path (see NOTE in the class remarks)
            return View();
        }
    }
}
