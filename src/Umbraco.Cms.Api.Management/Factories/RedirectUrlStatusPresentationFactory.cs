using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RedirectUrlStatusPresentationFactory : IRedirectUrlStatusPresentationFactory
{
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RedirectUrlStatusPresentationFactory(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webRoutingSettings = webRoutingSettings;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public RedirectUrlStatusResponseModel CreateViewModel()
    {
        RedirectStatus status = _webRoutingSettings.CurrentValue.DisableRedirectUrlTracking switch
        {
            true => RedirectStatus.Disabled,
            false => RedirectStatus.Enabled
        };

        return new RedirectUrlStatusResponseModel
        {
            Status = status,
            // TODO: Ensure that CurrentUser can be found when we use the new auth.
            UserIsAdmin = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false,
        };
    }
}
