using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Security;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Website.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        private readonly IUmbracoWebsiteSecurity _websiteSecurity;

        public UmbLoginController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoWebsiteSecurity websiteSecurity)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _websiteSecurity = websiteSecurity;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (_websiteSecurity.Login(model.Username, model.Password) == false)
            {
                // Don't add a field level error, just model level.
                ModelState.AddModelError("loginModel", "Invalid username or password");
                return CurrentUmbracoPage();
            }

            TempData["LoginSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // Validate the redirect url.
                // If it's not a local url we'll redirect to the root of the current site.
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage.AncestorOrSelf(1).Url(PublishedUrlProvider));
            }

            // Redirect to current page by default.
            return RedirectToCurrentUmbracoPage();
        }
    }
}
