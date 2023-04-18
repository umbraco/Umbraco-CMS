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
        model.AuthUrl = "/umbraco/management/api/v1.0/security/back-office";

        return View("/umbraco/UmbracoLogin/Index.cshtml", model);
    }
}
