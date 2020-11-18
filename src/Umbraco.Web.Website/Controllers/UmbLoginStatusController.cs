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
    // TOOO: reinstate [MemberAuthorize]
    public class UmbLoginStatusController : SurfaceController
    {
        private readonly IUmbracoWebsiteSecurity _websiteSecurity;

        public UmbLoginStatusController(IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches,
            IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IUmbracoWebsiteSecurity websiteSecurity)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _websiteSecurity = websiteSecurity;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult HandleLogout([Bind(Prefix = "logoutModel")]PostRedirectModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (_websiteSecurity.IsLoggedIn())
            {
                _websiteSecurity.LogOut();
            }

            TempData["LogoutSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                return Redirect(model.RedirectUrl);
            }

            // Redirect to current page by default.
            return RedirectToCurrentUmbracoPage();
        }
    }
}
