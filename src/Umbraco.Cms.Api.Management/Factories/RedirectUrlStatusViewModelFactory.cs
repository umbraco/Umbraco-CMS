using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Factories;

public class RedirectUrlStatusViewModelFactory : IRedirectUrlStatusViewModelFactory
{
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RedirectUrlStatusViewModelFactory(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webRoutingSettings = webRoutingSettings;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public RedirectUrlStatusViewModel CreateViewModel()
    {
        RedirectStatus status = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking switch
        {
            true => RedirectStatus.Disabled,
            false => RedirectStatus.Enabled
        };

        return new RedirectUrlStatusViewModel
        {
            Status = status,
            // TODO: Ensure that CurrentUser can be found when we use the new auth.
            UserIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false,
        };
    }
}
