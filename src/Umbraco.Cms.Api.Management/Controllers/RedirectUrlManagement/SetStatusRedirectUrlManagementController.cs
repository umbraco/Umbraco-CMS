using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiVersion("1.0")]
public class SetStatusRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IConfigManipulator _configManipulator;

    public SetStatusRedirectUrlManagementController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IConfigManipulator configManipulator)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _configManipulator = configManipulator;
    }

    // TODO: Consider if we should even allow this, or only allow using the appsettings
    // We generally don't want to edit the appsettings from our code.
    // But maybe there is a valid use case for doing it on the fly.
    [HttpPost("status")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> SetStatus(CancellationToken cancellationToken, [FromQuery] RedirectStatus status)
    {
        // TODO: uncomment this when auth is implemented.
        // var userIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin();
        // if (userIsAdmin is null or false)
        // {
        //     return Unauthorized();
        // }

        var enable = status switch
        {
            RedirectStatus.Enabled => true,
            RedirectStatus.Disabled => false
        };

        // For now I'm not gonna change this to limit breaking, but it's weird to have a "disabled" switch,
        // since you're essentially negating the boolean from the get go,
        // it's much easier to reason with enabled = false == disabled.
        await _configManipulator.SaveDisableRedirectUrlTrackingAsync(!enable);

        // Taken from the existing implementation in RedirectUrlManagementController
        // TODO this is ridiculous, but we need to ensure the configuration is reloaded, before this request is ended.
        // otherwise we can read the old value in GetEnableState.
        // The value is equal to JsonConfigurationSource.ReloadDelay
        Thread.Sleep(250);

        return Ok();
    }
}
