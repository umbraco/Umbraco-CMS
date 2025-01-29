using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Api.Management;

[BindProperties]
public class BackOfficeLoginModel
{
    /// <summary>
    /// Gets or sets the value of the "ReturnUrl" query parameter or defaults to the configured Umbraco directory.
    /// </summary>
    [FromQuery(Name = "ReturnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// The configured Umbraco directory.
    /// </summary>
    public string? UmbracoUrl { get; set; }

    public bool UserIsAlreadyLoggedIn { get; set; }
}

[ApiExplorerSettings(IgnoreApi=true)]
[Route(LoginPath)]
public class BackOfficeLoginController : Controller
{
    public const string LoginPath = "/umbraco/login";
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly GlobalSettings _globalSettings;

    public BackOfficeLoginController(
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
    }

    // GET
    public async Task<IActionResult> Index(CancellationToken cancellationToken, BackOfficeLoginModel model)
    {
        AuthenticateResult cookieAuthResult = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        if (cookieAuthResult.Succeeded)
        {
            model.UserIsAlreadyLoggedIn = true;
        }

        if (string.IsNullOrEmpty(model.UmbracoUrl))
        {
            model.UmbracoUrl = _hostingEnvironment.ToAbsolute(Constants.System.DefaultUmbracoPath);
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
