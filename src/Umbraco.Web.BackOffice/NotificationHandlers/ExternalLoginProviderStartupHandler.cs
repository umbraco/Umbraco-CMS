using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Web.BackOffice.NotificationHandlers;

/// <summary>
/// Invalidates backoffice sessions and clears external logins for removed providers if the external login
/// provider setup has changed.
/// </summary>
internal sealed class ExternalLoginProviderStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    public ExternalLoginProviderStartupHandler(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IRuntimeState runtimeState,
        IServerRoleAccessor serverRoleAccessor)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _runtimeState = runtimeState;
        _serverRoleAccessor = serverRoleAccessor;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        switch (_serverRoleAccessor.CurrentServerRole)
        {
            case ServerRole.Subscriber:
            case ServerRole.Unknown:
                return;
            case ServerRole.Single:
            case ServerRole.SchedulingPublisher:
            default:
                break;
        }

        _backOfficeExternalLoginProviders.InvalidateSessionsIfExternalLoginProvidersChanged();
    }
}
