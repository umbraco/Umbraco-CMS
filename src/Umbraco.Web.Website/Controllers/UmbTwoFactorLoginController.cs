using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.Website.Controllers;

/// <summary>
///     Surface controller that handles two-factor authentication flows for members, including provider
///     listing, code verification, and setup/disable operations.
/// </summary>
[UmbracoMemberAuthorize]
public class UmbTwoFactorLoginController : SurfaceController
{
    private readonly ILogger<UmbTwoFactorLoginController> _logger;
    private readonly IMemberManager _memberManager;
    private readonly IMemberSignInManager _memberSignInManager;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbTwoFactorLoginController" /> class.
    /// </summary>
    public UmbTwoFactorLoginController(
        ILogger<UmbTwoFactorLoginController> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager memberSignInManager,
        IMemberManager memberManager,
        ITwoFactorLoginService twoFactorLoginService)
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
    }

    /// <summary>
    ///     Retrieves the list of two-factor provider names valid for the member currently in the 2FA flow.
    /// </summary>
    /// <returns>
    ///     An <see cref="ObjectResult" /> containing the list of enabled provider names, or
    ///     <see cref="NotFoundResult" /> when no member is in the 2FA flow.
    /// </returns>
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> Get2FAProviders()
    {
        MemberIdentityUser? user = await _memberSignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null!)
        {
            _logger.LogWarning("Get2FAProviders :: No verified member found, returning 404");
            return NotFound();
        }

        IList<string> userFactors = await _memberManager.GetValidTwoFactorProvidersAsync(user);
        return new ObjectResult(userFactors);
    }

    /// <summary>
    ///     Verifies a two-factor authentication code for the member currently in the 2FA flow.
    /// </summary>
    /// <param name="model">The submitted two-factor code and provider details.</param>
    /// <param name="returnUrl">Optional URL to redirect to on successful verification.</param>
    /// <returns>
    ///     A redirect to the return URL on success; <see cref="NotFoundResult" /> when no member is in
    ///     the 2FA flow; otherwise the current page with validation errors added to ModelState.
    /// </returns>
    [AllowAnonymous]
    public async Task<IActionResult> Verify2FACode(Verify2FACodeModel model, string? returnUrl = null)
    {
        MemberIdentityUser? user = await _memberSignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null!)
        {
            _logger.LogWarning("Verify2FACode :: No verified member found, returning 404");
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            SignInResult result = await _memberSignInManager.TwoFactorSignInAsync(
                model.Provider,
                model.Code,
                model.IsPersistent,
                model.RememberClient);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(nameof(Verify2FACodeModel.Code), "Member is locked out");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(nameof(Verify2FACodeModel.Code), "Member is not allowed");
            }
            else
            {
                ModelState.AddModelError(nameof(Verify2FACodeModel.Code), "Invalid code");
            }
        }

        // We need to set this, to ensure we show the 2fa login page
        IEnumerable<string> providerNames =
            await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(user.Key);
        ViewData.SetTwoFactorProviderNames(providerNames);
        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     Validates a two-factor setup code and, on success, persists the two-factor login for the
    ///     currently authenticated member.
    /// </summary>
    /// <param name="providerName">The name of the two-factor provider being configured.</param>
    /// <param name="secret">The shared secret generated during setup.</param>
    /// <param name="code">The code the member has entered to verify the setup.</param>
    /// <param name="returnUrl">Optional URL to redirect to on success.</param>
    /// <returns>
    ///     A redirect to the return URL on success; otherwise the current page with a validation error
    ///     added to ModelState.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> ValidateAndSaveSetup(
        string providerName,
        string secret,
        string code,
        string? returnUrl = null)
    {
        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();

        var isValid = _twoFactorLoginService.ValidateTwoFactorSetup(providerName, secret, code);

        if (member is null || isValid == false)
        {
            ModelState.AddModelError(nameof(code), "Invalid Code");

            return CurrentUmbracoPage();
        }

        var twoFactorLogin = new TwoFactorLogin
        {
            Confirmed = true, Secret = secret, UserOrMemberKey = member.Key, ProviderName = providerName,
        };

        await _twoFactorLoginService.SaveAsync(twoFactorLogin);

        return RedirectToLocal(returnUrl);
    }

    /// <summary>
    ///     Disables a two-factor provider for the currently authenticated member.
    /// </summary>
    /// <param name="providerName">The name of the two-factor provider to disable.</param>
    /// <param name="returnUrl">Optional URL to redirect to on success.</param>
    /// <returns>
    ///     A redirect to the return URL on success; otherwise the current page.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Disable(string providerName, string? returnUrl = null)
    {
        MemberIdentityUser? member = await _memberManager.GetCurrentMemberAsync();

        var success = member is not null && await _twoFactorLoginService.DisableAsync(member.Key, providerName);

        if (!success)
        {
            return CurrentUmbracoPage();
        }

        return RedirectToLocal(returnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToCurrentUmbracoPage();
}
