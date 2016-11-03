using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
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
        private readonly DatabaseContext _databaseContext;
        private readonly UmbracoContext _umbracoContext;
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;

        public InstallController(UmbracoContext umbracoContext, DatabaseContext databaseContext, IRuntimeState runtime, ILogger logger)
		{
			_umbracoContext = umbracoContext;
		    _databaseContext = databaseContext;
            _runtime = runtime;
		    _logger = logger;
		}


        [HttpGet]
        public ActionResult Index()
        {
            if (_runtime.Level == RuntimeLevel.Run)
                return Redirect(SystemDirectories.Umbraco.EnsureEndsWith('/'));

            if (_runtime.Level == RuntimeLevel.Upgrade)
            {
                // Update ClientDependency version
                var clientDependencyConfig = new ClientDependencyConfiguration(_logger);
                var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

                var result = _umbracoContext.Security.ValidateCurrentUser(false);

                switch (result)
                {
                    case ValidateRequestAttempt.FailedNoPrivileges:
                    case ValidateRequestAttempt.FailedNoContextId:
                        return Redirect(SystemDirectories.Umbraco + "/AuthorizeUpgrade?redir=" + Server.UrlEncode(Request.RawUrl));
                }
            }

            // gen the install base url
            ViewBag.InstallApiBaseUrl = Url.GetUmbracoApiService("GetSetup", "InstallApi", "UmbracoInstall").TrimEnd("GetSetup");

            // get the base umbraco folder
            ViewBag.UmbracoBaseFolder = IOHelper.ResolveUrl(SystemDirectories.Umbraco);

            var ih = new InstallHelper(_umbracoContext, _databaseContext, _logger);
            ih.InstallStatus(false, "");

            // always ensure full path (see NOTE in the class remarks)
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "install/views/index.cshtml");
        }
    }
}
