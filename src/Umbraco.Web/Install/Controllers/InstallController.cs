using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;

namespace Umbraco.Web.Install.Controllers
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
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;

        public InstallController(IUmbracoContextAccessor umbracoContextAccessor, InstallHelper installHelper, IRuntimeState runtime, ILogger logger, IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _installHelper = installHelper;
            _runtime = runtime;
            _logger = logger;
            _globalSettings = globalSettings;
            _umbracoVersion = umbracoVersion;
        }

        [HttpGet]
        [StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable)]
        public ActionResult Index()
        {
            if (_runtime.Level == RuntimeLevel.Run)
                return Redirect(_globalSettings.UmbracoPath.EnsureEndsWith('/'));

            if (_runtime.Level == RuntimeLevel.Upgrade)
            {
                // Update ClientDependency version
                var clientDependencyConfig = new ClientDependencyConfiguration(_logger);
                var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                    _umbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");
                // Delete ClientDependency temp directories to make sure we get fresh caches
                var clientDependencyTempFilesDeleted = clientDependencyConfig.ClearTempFiles(HttpContext);

                var result = _umbracoContextAccessor.UmbracoContext.Security.ValidateCurrentUser(false);

                switch (result)
                {
                    case ValidateRequestAttempt.FailedNoPrivileges:
                    case ValidateRequestAttempt.FailedNoContextId:
                        return Redirect(_globalSettings.UmbracoPath + "/AuthorizeUpgrade?redir=" + Server.UrlEncode(Request.RawUrl));
                }
            }

            // gen the install base url
            ViewData.SetInstallApiBaseUrl(Url.GetUmbracoApiService("GetSetup", "InstallApi", "UmbracoInstall").TrimEnd("GetSetup"));

            // get the base umbraco folder
            ViewData.SetUmbracoBaseFolder(Current.IOHelper.ResolveUrl(_globalSettings.UmbracoPath));

            _installHelper.InstallStatus(false, "");

            // always ensure full path (see NOTE in the class remarks)
            return View(_globalSettings.Path.EnsureEndsWith('/') + "install/views/index.cshtml");
        }
    }
}
