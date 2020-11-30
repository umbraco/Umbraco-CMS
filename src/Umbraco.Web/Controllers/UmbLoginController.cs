using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        private readonly MembershipHelper _membershipHelper;

        public UmbLoginController()
        {
        }

        public UmbLoginController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger,
            MembershipHelper membershipHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger)
        {
            _membershipHelper = membershipHelper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (_membershipHelper.Login(model.Username, model.Password) == false)
            {
                //don't add a field level error, just model level
                ModelState.AddModelError("loginModel", "Invalid username or password");
                return CurrentUmbracoPage();
            }

            TempData["LoginSuccess"] = true;

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // validate the redirect URL
                // if it's not a local URL we'll redirect to the root of the current site
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage.AncestorOrSelf(1).Url());
            }

            //redirect to current page by default

            return RedirectToCurrentUmbracoPage();
        }
    }
}
