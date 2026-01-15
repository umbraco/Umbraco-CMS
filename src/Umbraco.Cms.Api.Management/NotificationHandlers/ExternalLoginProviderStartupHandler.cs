using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Api.Management.NotificationHandlers;

/// <summary>
/// Invalidates backoffice sessions and clears external logins for removed providers if the external login
/// provider setup has changed.
/// </summary>
internal sealed class ExternalLoginProviderStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalLoginProviderStartupHandler"/> class.
    /// </summary>
    public ExternalLoginProviderStartupHandler(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IRuntimeState runtimeState,
        IServerRoleAccessor serverRoleAccessor)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _runtimeState = runtimeState;
        _serverRoleAccessor = serverRoleAccessor;
    }

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run ||
            _serverRoleAccessor.CurrentServerRole == ServerRole.Subscriber)
        {
            return;
        }

        _backOfficeExternalLoginProviders.InvalidateSessionsIfExternalLoginProvidersChanged();
    }
}
