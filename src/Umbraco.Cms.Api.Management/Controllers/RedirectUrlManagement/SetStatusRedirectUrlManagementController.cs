using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

/// <summary>
/// Controller for setting the redirect URL tracking status. Retained for backwards compatibility only;
/// the endpoint no longer modifies any configuration.
/// </summary>
[ApiVersion("1.0")]
[Obsolete("This controller is deprecated and no longer modifies the configuration. Set the Umbraco:CMS:WebRouting:DisableRedirectUrlTracking configuration key instead. Scheduled for removal in Umbraco 19.")]
public class SetStatusRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    /// <summary>
    /// Deprecated. Returns an OK response without modifying any configuration. To toggle redirect URL tracking,
    /// set the <c>Umbraco:CMS:WebRouting:DisableRedirectUrlTracking</c> configuration key instead.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the HTTP request.</param>
    /// <param name="status">The redirect status (ignored).</param>
    /// <returns>An OK result.</returns>
    [HttpPost("status")]
    [EndpointSummary("Deprecated. No longer changes the redirect URL tracking status.")]
    [EndpointDescription("This endpoint is deprecated and no longer modifies the configuration. To toggle redirect URL tracking, set the Umbraco:CMS:WebRouting:DisableRedirectUrlTracking configuration key instead.")]
    [MapToApiVersion("1.0")]
    [Obsolete("This endpoint is deprecated and no longer modifies the configuration. Set the Umbraco:CMS:WebRouting:DisableRedirectUrlTracking configuration key instead. Scheduled for removal in Umbraco 19.")]
    public Task<IActionResult> SetStatus(CancellationToken cancellationToken, [FromQuery] RedirectStatus status)
        => Task.FromResult<IActionResult>(Ok());
}
