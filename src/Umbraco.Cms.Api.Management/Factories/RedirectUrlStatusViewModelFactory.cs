using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

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
        => new RedirectUrlStatusViewModel
        {
            Enabled = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking is false,
            // TODO: Ensure that CurrentUser can be found when we use the new auth.
            UserIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false,
        };
}
