using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Controllers
{
    [MemberAuthorize]
    public class UmbLoginStatusController : SurfaceController
    {
        // fixme - delete?
        public UmbLoginStatusController()
        {
        }

        public UmbLoginStatusController(UmbracoContext umbracoContext, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger) : base(umbracoContext, databaseFactory, services, applicationCache, logger, profilingLogger)
        {
        }

        [HttpPost]
        public ActionResult HandleLogout([Bind(Prefix = "logoutModel")]PostRedirectModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (Members.IsLoggedIn())
            {
                FormsAuthentication.SignOut();
            }

            TempData["LogoutSuccess"] = true;

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                return Redirect(model.RedirectUrl);
            }

            //redirect to current page by default

            return RedirectToCurrentUmbracoPage();
        }
    }
}
