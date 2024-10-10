using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

public class BackOfficeDefaultController : Controller
{
    private readonly IRuntime _umbracoRuntime;

    [ActivatorUtilitiesConstructor]
    public BackOfficeDefaultController(IRuntime umbracoRuntime)
        => _umbracoRuntime = umbracoRuntime;

    [Obsolete("Use the non obsoleted constructor instead. Scheduled to be removed in v17")]
    public BackOfficeDefaultController()
        : this(StaticServiceProvider.Instance.GetRequiredService<IRuntime>())
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // force authentication to occur since this is not an authorized endpoint
        // a user can not be authenticated if no users have been created yet, or the user repository is unavailable
        AuthenticateResult result = _umbracoRuntime.State.Level < RuntimeLevel.Upgrade
            ? AuthenticateResult.Fail("RuntimeLevel " + _umbracoRuntime.State.Level + " does not support authentication")
            : await this.AuthenticateBackOfficeAsync();

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
