using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        // fixme - delete?
        public UmbLoginController()
        {
        }

        public UmbLoginController(UmbracoContext umbracoContext, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, CacheHelper applicationCache, ILogger logger, ProfilingLogger profilingLogger)
            : base(umbracoContext, databaseFactory, services, applicationCache, logger, profilingLogger)
        {
        }

        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (Members.Login(model.Username, model.Password) == false)
            {
                //don't add a field level error, just model level
                ModelState.AddModelError("loginModel", "Invalid username or password");
                return CurrentUmbracoPage();
            }

            TempData["LoginSuccess"] = true;

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // validate the redirect url
                // if it's not a local url we'll redirect to the root of the current site
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage.AncestorOrSelf(1).Url);
            }

            //redirect to current page by default

            return RedirectToCurrentUmbracoPage();
        }
    }
}
