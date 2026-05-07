using Microsoft.AspNetCore.Authorization;
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

/// <summary>
///     Surface controller that handles member logout from the Login Status snippet.
/// </summary>
[UmbracoMemberAuthorize]
public class UmbLoginStatusController : SurfaceController
{
    private readonly IMemberSignInManager _signInManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbLoginStatusController" /> class.
    /// </summary>
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

    /// <summary>
    ///     Handles the logout form post, signing the current member out if they are authenticated.
    /// </summary>
    /// <param name="model">The posted model, optionally containing a redirect URL.</param>
    /// <returns>
    ///     A redirect to the supplied local URL when provided; otherwise a redirect to the current Umbraco page.
    /// </returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleLogout([Bind(Prefix = "logoutModel")] PostRedirectModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        var isLoggedIn = HttpContext.User.Identity?.IsAuthenticated ?? false;

        if (isLoggedIn)
        {
            await _signInManager.SignOutAsync();
        }

        TempData["LogoutSuccess"] = true;

        // If there is a specified path to redirect to and it is validated as a local URL, then use it.
        if (model.RedirectUrl.IsNullOrWhiteSpace() is false && Url.IsLocalUrl(model.RedirectUrl!))
        {
            return Redirect(model.RedirectUrl!);
        }

        // Redirect to current page by default.
        return RedirectToCurrentUmbracoPage();
    }

    // Route values carry encrypted, tamper-proof overrides for the posted model (see ValidateUmbracoFormRouteString).
    private void MergeRouteValuesToModel(PostRedirectModel model)
    {
        if (RouteData.Values.TryGetValue(nameof(PostRedirectModel.RedirectUrl), out var redirectUrl) && redirectUrl is not null)
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }
}
