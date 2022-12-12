using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.RedirectUrlManagement;

public class GetEnabledController : RedirectUrlManagementBaseController
{
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public GetEnabledController(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webRoutingSettings = webRoutingSettings;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet("status")]
    public Task<ActionResult<RedirectUrlStatusViewModel>> GetStatus() =>
        Task.FromResult<ActionResult<RedirectUrlStatusViewModel>>(new RedirectUrlStatusViewModel
        {
            Enabled = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking is false,
            UserIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false,
        });
}
