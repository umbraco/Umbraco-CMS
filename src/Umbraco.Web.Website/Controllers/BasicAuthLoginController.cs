using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Web.Website.Controllers;

/// <summary>
/// Provides a standalone server-rendered login page for basic authentication
/// when the backoffice SPA is not available (frontend-only deployments).
/// </summary>
[AllowAnonymous]
public class BasicAuthLoginController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnPath)
    {
        var model = new BasicAuthLoginModel { ReturnPath = returnPath };
        return View("/umbraco/BasicAuthLogin/Login.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string? username, string? password, string? returnPath)
    {
        IBackOfficeSignInManager? signInManager =
            HttpContext.RequestServices.GetService<IBackOfficeSignInManager>();

        if (signInManager is null)
        {
            return LoginView(returnPath, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return LoginView(returnPath, "Please enter a username and password.");
        }

        var signInResult = await signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirectOrHome(returnPath);
        }

        if (signInResult.IsLockedOut)
        {
            return LoginView(returnPath, "Your account has been locked out. Please try again later.");
        }

        if (signInResult.IsNotAllowed)
        {
            return LoginView(returnPath, "Your account is not allowed to sign in.");
        }

        if (signInResult.RequiresTwoFactor)
        {
            // Phase 2 will add 2FA support via a dedicated page.
            return LoginView(returnPath, "Two-factor authentication is required but is not supported for standalone login yet.");
        }

        return LoginView(returnPath, "Invalid username or password.");
    }

    private IActionResult LoginView(string? returnPath, string? errorMessage)
    {
        var model = new BasicAuthLoginModel
        {
            ReturnPath = returnPath,
            ErrorMessage = errorMessage,
        };
        return View("/umbraco/BasicAuthLogin/Login.cshtml", model);
    }

    private IActionResult LocalRedirectOrHome(string? returnPath)
    {
        if (!string.IsNullOrWhiteSpace(returnPath) && Url.IsLocalUrl(returnPath))
        {
            return LocalRedirect(returnPath);
        }

        return Redirect("/");
    }
}
