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
    public async Task<IActionResult> Index()
    {
        // force authentication to occur since this is not an authorized endpoint
        AuthenticateResult result = await this.AuthenticateBackOfficeAsync();

        /*
         TODO: Remove authentication check & clean controller in V14
         This is crossed out in V14 to allow the Backoffice to handle authentication itself whilst still allowing
         the old Umbraco.Web.UI executable now using the updated login screen from V13 to work with the old Backoffice.

        // if we are not authenticated then we need to redirect to the login page
        if (!result.Succeeded)
        {
            return RedirectToLogin(null);
        }
        */

        ViewResult defaultView = DefaultView();

        return defaultView;

        // return await RenderDefaultOrProcessExternalLoginAsync(
        //     result,
        //     () => defaultView);
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
