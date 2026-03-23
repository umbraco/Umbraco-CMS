using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Api.Management;

/// <summary>
/// Provides endpoints for managing back office user authentication and login operations.
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
[Route(LoginPath)]
public class BackOfficeLoginController : Controller
{
    public const string LoginPath = "/umbraco/login";
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly GlobalSettings _globalSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeLoginController"/> class.
    /// </summary>
    /// <param name="globalSettings">A snapshot of the application's global settings options.</param>
    /// <param name="hostingEnvironment">The current hosting environment for the application.</param>
    public BackOfficeLoginController(
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
    }

    // GET
    /// <summary>
    /// Handles the GET request for the back office login page.
    /// If the user is already authenticated, updates the model accordingly.
    /// Ensures the return URL is a relative path and sets default values if necessary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <param name="model">The model containing login information and the return URL.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that renders the login view with the model, or a bad request result if the return URL is invalid.
    /// </returns>
    public async Task<IActionResult> Index(CancellationToken cancellationToken, BackOfficeLoginModel model)
    {
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        if (cookieAuthResult.Succeeded)
        {
            model.UserIsAlreadyLoggedIn = true;
        }

        if (string.IsNullOrEmpty(model.UmbracoUrl))
        {
            model.UmbracoUrl = _hostingEnvironment.GetBackOfficePath();
        }

        if (string.IsNullOrEmpty(model.ReturnUrl))
        {
            model.ReturnUrl = model.UmbracoUrl;
        }

        if ( Uri.TryCreate(model.ReturnUrl, UriKind.Relative, out _) is false) // Needs to test for relative and not absolute, as /whatever/ is an absolute path on linux
        {
            return BadRequest("ReturnUrl must be a relative path.");
        }

        return View("/umbraco/UmbracoLogin/Index.cshtml", model);
    }
}
