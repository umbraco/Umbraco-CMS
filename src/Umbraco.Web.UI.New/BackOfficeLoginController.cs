using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.UI;

[BindProperties]
public class BackOfficeLoginModel
{
    [FromQuery(Name = "ReturnUrl")]
    public string? ReturnUrl { get; set; }

    public string AuthUrl { get; set; } = string.Empty;
}

[ApiExplorerSettings(IgnoreApi=true)]
[Route("/umbraco/login")]
public class BackOfficeLoginController : Controller
{
    private readonly LinkGenerator _linkGenerator;

    public BackOfficeLoginController(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    // GET
    public IActionResult Index(BackOfficeLoginModel model)
    {
        var authUrl = _linkGenerator.GetUmbracoApiServiceBaseUrl<AuthenticationController>(
            controller => controller.PostLogin(new LoginModel()));

        model.AuthUrl = authUrl ?? string.Empty;

        return View("/umbraco/UmbracoLogin/Index.cshtml", model);
    }
}
