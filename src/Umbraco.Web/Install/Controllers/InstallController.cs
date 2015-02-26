using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Security;

namespace Umbraco.Web.Install.Controllers
{
    /// <summary>
    /// The MVC Installation controller
    /// </summary>
    /// <remarks>
    /// NOTE: All views must have their full paths as we do not have a custom view engine for the installation views!
    /// </remarks>
    [InstallAuthorizeAttribute]
    public class InstallController : Controller
    {
        private readonly UmbracoContext _umbracoContext;

        public InstallController()
            : this(UmbracoContext.Current)
        {

        }

        public InstallController(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }


        [HttpGet]
        public ActionResult Index()
        {
            // If this is not an upgrade we will log in with the default user.
            // It's not considered an upgrade if the ConfigurationStatus is missing or empty or if the db is not configured.
            var configuredVersion = GlobalSettings.ConfigurationStatus;

            if (string.IsNullOrWhiteSpace(configuredVersion) == false
                && ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
            {
                var semVerVersion = string.Empty;

                // There may be a versionComment
                var versionParts = configuredVersion.Split('.');
                if (versionParts.Length >= 3)
                    semVerVersion = string.Format("{0}.{1}.{2}", versionParts[0], versionParts[1], versionParts[2]);

                var versionComment = string.Empty;
                if (versionParts.Length == 4)
                    versionComment = versionParts[3];

                Version current;
                if (Version.TryParse(semVerVersion, out current))
                {
                    //check if we are on the current version, and not let the installer execute
                    if (current == UmbracoVersion.Current && string.IsNullOrWhiteSpace(versionComment) == false && UmbracoVersion.CurrentComment == versionComment)
                    {
                        return Redirect(SystemDirectories.Umbraco.EnsureEndsWith('/'));
                    }
                }

                var result = _umbracoContext.Security.ValidateCurrentUser(false);

                switch (result)
                {
                    case ValidateRequestAttempt.FailedNoPrivileges:
                    case ValidateRequestAttempt.FailedNoContextId:
                        return Redirect(SystemDirectories.Umbraco + "/AuthorizeUpgrade?redir=" + Server.UrlEncode(Request.RawUrl));
                }
            }

            //gen the install base url
            ViewBag.InstallApiBaseUrl = Url.GetUmbracoApiService("GetSetup", "InstallApi", "UmbracoInstall").TrimEnd("GetSetup");

            //get the base umbraco folder
            ViewBag.UmbracoBaseFolder = IOHelper.ResolveUrl(SystemDirectories.Umbraco);

            InstallHelper ih = new InstallHelper(_umbracoContext);
            ih.InstallStatus(false, "");

            //always ensure full path (see NOTE in the class remarks)
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "install/views/index.cshtml");
        }
    }
}
