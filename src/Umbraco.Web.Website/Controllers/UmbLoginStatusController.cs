using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers;

[UmbracoMemberAuthorize]
public class UmbLoginStatusController : SurfaceController
{
    private readonly IMemberSignInManager _signInManager;

    public UmbLoginStatusController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        => _signInManager = signInManager;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleLogout([Bind(Prefix = "logoutModel")] PostRedirectModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        var isLoggedIn = HttpContext.User.Identity?.IsAuthenticated ?? false;

        if (isLoggedIn)
        {
            await _signInManager.SignOutAsync();
        }

        TempData["LogoutSuccess"] = true;

        // If there is a specified path to redirect to then use it.
        if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
        {
            return Redirect(model.RedirectUrl!);
        }

        // Redirect to current page by default.
        return RedirectToCurrentUmbracoPage();
    }
}
