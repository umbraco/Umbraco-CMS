using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.Website.Controllers;

[UmbracoMemberAuthorize]
public class UmbExternalLoginController : SurfaceController
{
    private readonly ILogger<UmbExternalLoginController> _logger;
    private readonly IMemberManager _memberManager;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly IOptions<SecuritySettings> _securitySettings;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    public UmbExternalLoginController(
        ILogger<UmbExternalLoginController> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        ITwoFactorLoginService twoFactorLoginService,
        IOptions<SecuritySettings> securitySettings)
        : base(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider)
    {
        _logger = logger;
        _memberSignInManager = memberSignInManager;
        _memberManager = memberManager;
        _twoFactorLoginService = twoFactorLoginService;
        _securitySettings = securitySettings;
    }

    /// <summary>
    ///     Endpoint used to redirect to a specific login provider. This endpoint is used from the Login Macro snippet.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (returnUrl.IsNullOrWhiteSpace())
        {
            returnUrl = Request.GetEncodedPathAndQuery();
        }

        var wrappedReturnUrl =
            Url.SurfaceAction(nameof(ExternalLoginCallback), this.GetControllerName(), new { returnUrl });

        AuthenticationProperties properties =
            _memberSignInManager.ConfigureExternalAuthenticationProperties(provider, wrappedReturnUrl);

        return Challenge(properties, provider);
    }

    /// <summary>
    ///     Endpoint used my the login provider to call back to our solution.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl)
    {
        var errors = new List<string>();

        ExternalLoginInfo? loginInfo = await _memberSignInManager.GetExternalLoginInfoAsync();
        if (loginInfo is null)
        {
            errors.Add("Invalid response from the login provider");
        }
        else
        {
            SignInResult result = await _memberSignInManager.ExternalLoginSignInAsync(
                loginInfo,
                false,
                _securitySettings.Value.MemberBypassTwoFactorForExternalLogins);

            if (result == SignInResult.Success)
            {
                // Update any authentication tokens if succeeded
                await _memberSignInManager.UpdateExternalAuthenticationTokensAsync(loginInfo);

                return RedirectToLocal(returnUrl);
            }

            if (result == SignInResult.TwoFactorRequired)
            {
                MemberIdentityUser attemptedUser =
                    await _memberManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
                if (attemptedUser == null!)
                {
                    return new ValidationErrorResult(
                        $"No local user found for the login provider {loginInfo.LoginProvider} - {loginInfo.ProviderKey}");
                }

                IEnumerable<string> providerNames =
                    await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(attemptedUser.Key);
                ViewData.SetTwoFactorProviderNames(providerNames);

                return CurrentUmbracoPage();
            }

            if (result == SignInResult.LockedOut)
            {
                errors.Add(
                    $"The local member {loginInfo.Principal.Identity?.Name} for the external provider {loginInfo.ProviderDisplayName} is locked out.");
            }
            else if (result == SignInResult.NotAllowed)
            {
                // This occurs when SignInManager.CanSignInAsync fails which is when RequireConfirmedEmail , RequireConfirmedPhoneNumber or RequireConfirmedAccount fails
                // however since we don't enforce those rules (yet) this shouldn't happen.
                errors.Add(
                    $"The member {loginInfo.Principal.Identity?.Name} for the external provider {loginInfo.ProviderDisplayName} has not confirmed their details and cannot sign in.");
            }
            else if (result == SignInResult.Failed)
            {
                // Failed only occurs when the user does not exist
                errors.Add("The requested provider (" + loginInfo.LoginProvider +
                           ") has not been linked to an account, the provider must be linked before it can be used.");
            }
            else if (result == MemberSignInManager.ExternalLoginSignInResult.NotAllowed)
            {
                // This occurs when the external provider has approved the login but custom logic in OnExternalLogin has denined it.
                errors.Add(
                    $"The user {loginInfo.Principal.Identity?.Name} for the external provider {loginInfo.ProviderDisplayName} has not been accepted and cannot sign in.");
            }
            else if (result == MemberSignInManager.AutoLinkSignInResult.FailedNotLinked)
            {
                errors.Add("The requested provider (" + loginInfo.LoginProvider +
                           ") has not been linked to an account, the provider must be linked from the back office.");
            }
            else if (result == MemberSignInManager.AutoLinkSignInResult.FailedNoEmail)
            {
                errors.Add(
                    $"The requested provider ({loginInfo.LoginProvider}) has not provided the email claim {ClaimTypes.Email}, the account cannot be linked.");
            }
            else if (result is MemberSignInManager.AutoLinkSignInResult autoLinkSignInResult &&
                     autoLinkSignInResult.Errors.Count > 0)
            {
                errors.AddRange(autoLinkSignInResult.Errors);
            }
            else if (!result.Succeeded)
            {
                // this shouldn't occur, the above should catch the correct error but we'll be safe just in case
                errors.Add($"An unknown error with the requested provider ({loginInfo.LoginProvider}) occurred.");
            }
        }

        if (errors.Count > 0)
        {
            ViewData.SetExternalSignInProviderErrors(
                new BackOfficeExternalLoginProviderErrors(
                    loginInfo?.LoginProvider,
                    errors));

            return CurrentUmbracoPage();
        }

        return RedirectToLocal(returnUrl);
    }

    private void AddModelErrors(IdentityResult result, string prefix = "")
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(prefix, error.Description);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LinkLogin(string provider, string? returnUrl = null)
    {
        if (returnUrl.IsNullOrWhiteSpace())
        {
            returnUrl = Request.GetEncodedPathAndQuery();
        }

        var wrappedReturnUrl =
            Url.SurfaceAction(nameof(ExternalLinkLoginCallback), this.GetControllerName(), new { returnUrl });

        // Configures the redirect URL and user identifier for the specified external login including xsrf data
        AuthenticationProperties properties =
            _memberSignInManager.ConfigureExternalAuthenticationProperties(
                provider,
                wrappedReturnUrl,
                _memberManager.GetUserId(User));

        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLinkLoginCallback(string returnUrl)
    {
        MemberIdentityUser user = await _memberManager.GetUserAsync(User);
        string? loginProvider = null;
        var errors = new List<string>();
        if (user == null!)
        {
            // ... this should really not happen
            errors.Add("Local user does not exist");
        }
        else
        {
            ExternalLoginInfo? info =
                await _memberSignInManager.GetExternalLoginInfoAsync(await _memberManager.GetUserIdAsync(user));

            if (info == null)
            {
                // Add error and redirect for it to be displayed
                errors.Add("An error occurred, could not get external login info");
            }
            else
            {
                loginProvider = info.LoginProvider;
                IdentityResult addLoginResult = await _memberManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    // Update any authentication tokens if succeeded
                    await _memberSignInManager.UpdateExternalAuthenticationTokensAsync(info);

                    return RedirectToLocal(returnUrl);
                }

                // Add errors and redirect for it to be displayed
                errors.AddRange(addLoginResult.Errors.Select(x => x.Description));
            }
        }

        ViewData.SetExternalSignInProviderErrors(
            new BackOfficeExternalLoginProviderErrors(
                loginProvider,
                errors));
        return CurrentUmbracoPage();
    }

    private IActionResult RedirectToLocal(string returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToCurrentUmbracoPage();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disassociate(string provider, string providerKey, string? returnUrl = null)
    {
        if (returnUrl.IsNullOrWhiteSpace())
        {
            returnUrl = Request.GetEncodedPathAndQuery();
        }

        MemberIdentityUser user = await _memberManager.FindByIdAsync(User.Identity?.GetUserId());

        IdentityResult result = await _memberManager.RemoveLoginAsync(user, provider, providerKey);

        if (result.Succeeded)
        {
            await _memberSignInManager.SignInAsync(user, false);
            return RedirectToLocal(returnUrl!);
        }

        AddModelErrors(result);
        return CurrentUmbracoPage();
    }
}
