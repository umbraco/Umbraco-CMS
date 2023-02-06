using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.UI;

[ApiExplorerSettings(IgnoreApi=true)]
[Route("/umbraco/login")]
public class BackOfficeLoginController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View("/umbraco/UmbracoLogin/Index.cshtml");
    }
}
