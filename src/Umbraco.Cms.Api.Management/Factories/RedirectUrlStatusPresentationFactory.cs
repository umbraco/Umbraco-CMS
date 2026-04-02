using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.RedirectUrlManagement;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory responsible for creating instances of <see cref="RedirectUrlStatusPresentation"/>,
/// which represent the presentation details of redirect URL statuses in the management API.
/// </summary>
public class RedirectUrlStatusPresentationFactory : IRedirectUrlStatusPresentationFactory
{
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlStatusPresentationFactory"/> class.
    /// </summary>
    /// <param name="webRoutingSettings">An <see cref="IOptionsMonitor{WebRoutingSettings}"/> instance providing access to web routing settings.</param>
    /// <param name="backOfficeSecurityAccessor">An <see cref="IBackOfficeSecurityAccessor"/> instance for accessing back office security information.</param>
    public RedirectUrlStatusPresentationFactory(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webRoutingSettings = webRoutingSettings;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a view model containing the current redirect URL tracking status and whether the current user is an administrator.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Api.Management.Models.RedirectUrlStatusResponseModel" /> with the redirect URL tracking status and admin state of the current user.</returns>
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
