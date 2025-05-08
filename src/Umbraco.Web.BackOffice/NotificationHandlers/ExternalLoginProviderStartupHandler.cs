using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Web.BackOffice.NotificationHandlers;

/// <summary>
/// Invalidates backoffice sessions and clears external logins for removed providers if the external login
/// provider setup has changed.
/// </summary>
public class ExternalLoginProviderStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IRuntimeState _runtimeState;

    public ExternalLoginProviderStartupHandler(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IRuntimeState runtimeState)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _runtimeState = runtimeState;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level == RuntimeLevel.Run)
        {
            _backOfficeExternalLoginProviders.InvalidateSessionsIfExternalLoginProvidersChanged();
        }
    }
}
