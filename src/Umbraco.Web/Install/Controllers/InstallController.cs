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
            if (ApplicationContext.Current.IsConfigured)
            {
                return Redirect(SystemDirectories.Umbraco.EnsureEndsWith('/'));   
            }

            if (ApplicationContext.Current.IsUpgrading)
            {
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
