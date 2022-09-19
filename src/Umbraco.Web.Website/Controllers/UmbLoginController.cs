using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.Website.Controllers;

public class UmbLoginController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberSignInManager _signInManager;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    [ActivatorUtilitiesConstructor]
    public UmbLoginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager,
        IMemberManager memberManager,
        ITwoFactorLoginService twoFactorLoginService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _signInManager = signInManager;
        _memberManager = memberManager;
        _twoFactorLoginService = twoFactorLoginService;
    }

    [Obsolete("Use ctor with all params")]
    public UmbLoginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager)
        : this(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider,
            signInManager,
            StaticServiceProvider.Instance.GetRequiredService<IMemberManager>(),
            StaticServiceProvider.Instance.GetRequiredService<ITwoFactorLoginService>())
    {
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleLogin([Bind(Prefix = "loginModel")] LoginModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        // Sign the user in with username/password, this also gives a chance for developers to
        // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
        SignInResult result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, true);

        if (result.Succeeded)
        {
            TempData["LoginSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // Validate the redirect URL.
                // If it's not a local URL we'll redirect to the root of the current site.
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage!.AncestorOrSelf(1)!.Url(PublishedUrlProvider));
            }

            // Redirect to current URL by default.
            // This is different from the current 'page' because when using Public Access the current page
            // will be the login page, but the URL will be on the requested page so that's where we need
            // to redirect too.
            return RedirectToCurrentUmbracoUrl();
        }

        if (result.RequiresTwoFactor)
        {
            MemberIdentityUser attemptedUser = await _memberManager.FindByNameAsync(model.Username);
            if (attemptedUser == null!)
            {
                return new ValidationErrorResult(
                    $"No local member found for username {model.Username}");
            }

            IEnumerable<string> providerNames =
                await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(attemptedUser.Key);
            ViewData.SetTwoFactorProviderNames(providerNames);
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError("loginModel", "Member is locked out");
        }
        else if (result.IsNotAllowed)
        {
            ModelState.AddModelError("loginModel", "Member is not allowed");
        }
        else
        {
            ModelState.AddModelError("loginModel", "Invalid username or password");
        }

        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     We pass in values via encrypted route values so they cannot be tampered with and merge them into the model for use
    /// </summary>
    /// <param name="model"></param>
    private void MergeRouteValuesToModel(LoginModel model)
    {
        if (RouteData.Values.TryGetValue(nameof(LoginModel.RedirectUrl), out var redirectUrl) && redirectUrl != null)
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }
}
