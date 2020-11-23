using System.Threading.Tasks;
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
    [UmbracoMemberAuthorize]
    public class UmbLoginStatusController : SurfaceController
    {
        private readonly IUmbracoWebsiteSecurityAccessor _websiteSecurityAccessor;

        public UmbLoginStatusController(IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches,
            IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IUmbracoWebsiteSecurityAccessor websiteSecurityAccessor)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _websiteSecurityAccessor = websiteSecurityAccessor;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleLogout([Bind(Prefix = "logoutModel")]PostRedirectModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (_websiteSecurityAccessor.WebsiteSecurity.IsLoggedIn())
            {
                await _websiteSecurityAccessor.WebsiteSecurity.LogOutAsync();
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
