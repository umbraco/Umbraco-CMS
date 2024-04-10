using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

public class BackOfficeDefaultController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // force authentication to occur since this is not an authorized endpoint
        AuthenticateResult result = await this.AuthenticateBackOfficeAsync();

        // if we are not authenticated then we need to redirect to the login page
        if (!result.Succeeded)
        {
            RedirectToAction("Login", "Backoffice");
        }

        ViewResult defaultView = DefaultView();

        return defaultView;
    }

    /// <summary>
    ///     Returns the default view for the BackOffice
    /// </summary>
    /// <returns>The default view currently /umbraco/UmbracoBackOffice/Default.cshtml</returns>
    public ViewResult DefaultView()
    {
        var viewPath = Path.Combine(Constants.SystemDirectories.Umbraco, Constants.Web.Mvc.BackOfficeArea, nameof(Index) + ".cshtml")
            .Replace("\\", "/"); // convert to forward slashes since it's a virtual path
        return View(viewPath);
    }
}
