using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.ManagementApi.ViewModels.RedirectManagement;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.RedirectManagement;


public class GetEnabledController : RedirectManagementBaseController
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
    public Task<ActionResult<RedirectStatusViewModel>> GetStatus() =>
        Task.FromResult<ActionResult<RedirectStatusViewModel>>(new RedirectStatusViewModel
        {
            Enabled = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking is false,
            UserIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false
        });
}
