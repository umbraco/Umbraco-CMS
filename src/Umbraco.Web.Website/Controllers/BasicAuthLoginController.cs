using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.Website.Controllers;

/// <summary>
/// Provides a standalone server-rendered login page for basic authentication
/// when the backoffice SPA is not available (frontend-only deployments).
/// </summary>
/// <remarks>
/// This controller is used by <c>BasicAuthenticationMiddleware</c> when <c>RedirectToLoginPage</c> is enabled.
/// It supports username/password login and two-factor authentication via <see cref="IBackOfficeSignInManager"/>.
/// Dependencies are resolved from request services rather than constructor injection so that the controller
/// can be activated even when <c>AddBackOfficeSignIn()</c> has not been called.
/// All actions return 404 when basic authentication is not enabled, preventing the login endpoint
/// from being used as a backdoor sign-in mechanism.
/// </remarks>
[AllowAnonymous]
public class BasicAuthLoginController : Controller
{
    /// <summary>
    /// Renders the login form.
    /// </summary>
    /// <param name="returnPath">The local URL to redirect to after successful login.</param>
    /// <returns>The login view, or 404 if basic auth is not enabled.</returns>
    [HttpGet]
    public async Task<IActionResult> Login(string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        return await LoginView(returnPath);
    }

    /// <summary>
    /// Processes a username/password login attempt.
    /// </summary>
    /// <param name="username">The backoffice username.</param>
    /// <param name="password">The backoffice password.</param>
    /// <param name="returnPath">The local URL to redirect to after successful login.</param>
    /// <returns>A redirect on success, or the login view with an error message on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string? username, string? password, string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return await LoginView(returnPath, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return await LoginView(returnPath, "Please enter a username and password.");
        }

        SignInResult signInResult = await signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirectOrHome(returnPath);
        }

        if (signInResult.IsLockedOut)
        {
            return await LoginView(returnPath, "Your account has been locked out. Please try again later.");
        }

        if (signInResult.IsNotAllowed)
        {
            return await LoginView(returnPath, "Your account is not allowed to sign in.");
        }

        if (signInResult.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(TwoFactor), new { returnPath });
        }

        return await LoginView(returnPath, "Invalid username or password.");
    }

    /// <summary>
    /// Renders the two-factor authentication code entry form.
    /// </summary>
    /// <param name="returnPath">The local URL to redirect to after successful verification.</param>
    /// <returns>The 2FA view, or a redirect to login if no user is in the 2FA flow.</returns>
    [HttpGet]
    public async Task<IActionResult> TwoFactor(string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return await LoginView(returnPath, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
        }

        BackOfficeIdentityUser? user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            // No user in 2FA flow — session may have expired, redirect back to login.
            return RedirectToAction(nameof(Login), new { returnPath });
        }

        ITwoFactorLoginService twoFactorService =
            HttpContext.RequestServices.GetRequiredService<ITwoFactorLoginService>();

        IEnumerable<string> providerNames = await twoFactorService.GetEnabledTwoFactorProviderNamesAsync(user.Key);

        var model = new BasicAuthTwoFactorModel
        {
            ReturnPath = returnPath,
            ProviderNames = providerNames,
        };
        return View("/umbraco/BasicAuthLogin/TwoFactor.cshtml", model);
    }

    /// <summary>
    /// Processes a two-factor authentication code submission.
    /// </summary>
    /// <param name="provider">The 2FA provider name (e.g. "UmbracoUserAppAuthenticator").</param>
    /// <param name="code">The verification code from the authenticator app.</param>
    /// <param name="returnPath">The local URL to redirect to after successful verification.</param>
    /// <returns>A redirect on success, or the 2FA view with an error message on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(string? provider, string? code, string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return await LoginView(returnPath, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
        }

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(code))
        {
            return await TwoFactorView(signInManager, returnPath, "Please enter a verification code.");
        }

        SignInResult signInResult = await signInManager.TwoFactorSignInAsync(provider, code, isPersistent: false, rememberClient: false);

        if (signInResult.Succeeded)
        {
            return LocalRedirectOrHome(returnPath);
        }

        if (signInResult.IsLockedOut)
        {
            return await LoginView(returnPath, "Your account has been locked out. Please try again later.");
        }

        return await TwoFactorView(signInManager, returnPath, "Invalid verification code. Please try again.");
    }

    /// <summary>
    /// Initiates an external login challenge (e.g. Google, Microsoft) by redirecting to the provider.
    /// </summary>
    /// <param name="provider">The external authentication provider name.</param>
    /// <param name="returnPath">The local URL to redirect to after successful login.</param>
    /// <returns>A challenge result that redirects to the external provider, or 404 if basic auth is disabled.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return NotFound();
        }

        var callbackUrl = Url.Action(nameof(ExternalLoginCallback), "BasicAuthLogin", new { returnPath });
        AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from an external login provider after authentication.
    /// </summary>
    /// <param name="returnPath">The local URL to redirect to after successful login.</param>
    /// <returns>A redirect on success, or the login view with an error message on failure.</returns>
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnPath)
    {
        if (IsBasicAuthEnabled() is false)
        {
            return NotFound();
        }

        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return await LoginView(returnPath, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
        }

        ExternalLoginInfo? loginInfo = await signInManager.GetExternalLoginInfoAsync();
        if (loginInfo is null)
        {
            return await LoginView(returnPath, "Invalid response from the external login provider.");
        }

        SignInResult signInResult = await signInManager.ExternalLoginSignInAsync(loginInfo, isPersistent: false);

        if (signInResult.Succeeded)
        {
            await signInManager.UpdateExternalAuthenticationTokensAsync(loginInfo);
            return LocalRedirectOrHome(returnPath);
        }

        if (signInResult.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(TwoFactor), new { returnPath });
        }

        if (signInResult.IsLockedOut)
        {
            return await LoginView(returnPath, "Your account has been locked out. Please try again later.");
        }

        return await LoginView(returnPath, $"Unable to sign in with {loginInfo.ProviderDisplayName}.");
    }

    /// <summary>
    /// Builds the 2FA view with the user's enabled provider names and an optional error message.
    /// Redirects to login if no user is currently in the 2FA flow.
    /// </summary>
    private async Task<IActionResult> TwoFactorView(IBackOfficeSignInManager signInManager, string? returnPath, string? errorMessage)
    {
        BackOfficeIdentityUser? user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login), new { returnPath });
        }

        ITwoFactorLoginService twoFactorService =
            HttpContext.RequestServices.GetRequiredService<ITwoFactorLoginService>();

        IEnumerable<string> providerNames = await twoFactorService.GetEnabledTwoFactorProviderNamesAsync(user.Key);

        var model = new BasicAuthTwoFactorModel
        {
            ReturnPath = returnPath,
            ErrorMessage = errorMessage,
            ProviderNames = providerNames,
        };
        return View("/umbraco/BasicAuthLogin/TwoFactor.cshtml", model);
    }

    /// <summary>
    /// Builds the login view with an optional error message and available external login providers.
    /// </summary>
    private async Task<ViewResult> LoginView(string? returnPath, string? errorMessage = null)
    {
        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        IEnumerable<AuthenticationScheme> externalProviders = signInManager is not null
            ? await signInManager.GetExternalAuthenticationSchemesAsync()
            : [];

        var model = new BasicAuthLoginModel
        {
            ReturnPath = returnPath,
            ErrorMessage = errorMessage,
            ExternalLoginProviders = externalProviders,
        };
        return View("/umbraco/BasicAuthLogin/Login.cshtml", model);
    }

    /// <summary>
    /// Redirects to the return path if it is a valid local URL, otherwise redirects to the site root.
    /// </summary>
    private IActionResult LocalRedirectOrHome(string? returnPath)
    {
        if (!string.IsNullOrWhiteSpace(returnPath) && Url.IsLocalUrl(returnPath))
        {
            return LocalRedirect(returnPath);
        }

        return Redirect("/");
    }

    /// <summary>
    /// Checks whether basic authentication is enabled via <see cref="IBasicAuthService"/>.
    /// Returns false if the service is not registered or basic auth is disabled.
    /// </summary>
    private bool IsBasicAuthEnabled()
    {
        IBasicAuthService? basicAuthService = HttpContext.RequestServices.GetService<IBasicAuthService>();
        return basicAuthService?.IsBasicAuthEnabled() == true;
    }
}
