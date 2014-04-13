using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //if this is not an upgrade we will log in with the default user.
            // It's not considered an upgrade if the ConfigurationStatus is missing or empty or if the db is not configured.
            if (string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false
                && ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
            {
                Version current;
                if (Version.TryParse(GlobalSettings.ConfigurationStatus, out current))
                {
                    //check if we are on the current version, and not let the installer execute
                    if (current == UmbracoVersion.Current)
                    {
                        return Redirect(SystemDirectories.Umbraco.EnsureEndsWith('/'));
                    }    
                }

                var result = _umbracoContext.Security.ValidateCurrentUser(false);

                switch (result)
                {
                    case ValidateRequestAttempt.FailedNoPrivileges:
                    case ValidateRequestAttempt.FailedTimedOut:
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
