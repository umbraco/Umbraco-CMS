using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

/// <summary>
/// Provides default actions for managing back office security within the Umbraco CMS.
/// </summary>
public class BackOfficeDefaultController : Controller
{
    private readonly IRuntime _umbracoRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeDefaultController"/> class.
    /// </summary>
    /// <param name="umbracoRuntime">An instance of <see cref="IRuntime"/> representing the Umbraco runtime environment.</param>
    [ActivatorUtilitiesConstructor]
    public BackOfficeDefaultController(IRuntime umbracoRuntime)
        => _umbracoRuntime = umbracoRuntime;

    /// <summary>
    /// Retrieves the default configuration and settings for the Umbraco back office.
    /// This endpoint also ensures authentication is attempted, redirecting unauthenticated users to the login page.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the action, which is typically the default back office view or a redirect to login if authentication fails.</returns>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Gets the back office default configuration.")]
    [EndpointDescription("Gets the default configuration and settings for the Umbraco back office.")]
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
