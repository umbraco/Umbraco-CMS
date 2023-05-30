using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Api.Management;

[BindProperties]
public class
    BackOfficeLoginModel
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
}

[ApiExplorerSettings(IgnoreApi=true)]
[Route("/umbraco/login")]
public class BackOfficeLoginController : Controller
{
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
    public IActionResult Index(BackOfficeLoginModel model)
    {
        if (string.IsNullOrEmpty(model.UmbracoUrl))
        {
            model.UmbracoUrl = _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoPath);
        }

        if (string.IsNullOrEmpty(model.ReturnUrl))
        {
            model.ReturnUrl = model.UmbracoUrl;
        }

        return View("/umbraco/UmbracoLogin/Index.cshtml", model);
    }
}
