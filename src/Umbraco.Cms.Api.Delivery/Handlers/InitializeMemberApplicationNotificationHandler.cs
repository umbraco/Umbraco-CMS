using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Delivery.Handlers;

internal sealed class InitializeMemberApplicationNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IMemberApplicationManager _memberApplicationManager;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<InitializeMemberApplicationNotificationHandler> _logger;
    private readonly DeliveryApiSettings _deliveryApiSettings;

    public InitializeMemberApplicationNotificationHandler(
        IMemberApplicationManager memberApplicationManager,
        IRuntimeState runtimeState,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<InitializeMemberApplicationNotificationHandler> logger)
    {
        _memberApplicationManager = memberApplicationManager;
        _runtimeState = runtimeState;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        if (_deliveryApiSettings.MemberAuthorization?.AuthorizationCodeFlow?.Enabled is not true)
        {
            await _memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        if (ValidateRedirectUrls(_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LoginRedirectUrls) is false)
        {
            await _memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        if (_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls.Any()
            && ValidateRedirectUrls(_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls) is false)
        {
            await _memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        await _memberApplicationManager.EnsureMemberApplicationAsync(
            _deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LoginRedirectUrls,
            _deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls,
            cancellationToken);
    }

    private bool ValidateRedirectUrls(Uri[] redirectUrls)
    {
        if (redirectUrls.Any() is false)
        {
            _logger.LogWarning("No redirect URLs defined for Delivery API member authentication - cannot enable member authentication");
            return false;
        }

        if (redirectUrls.All(url => url.IsAbsoluteUri) is false)
        {
            _logger.LogWarning("All redirect URLs defined for Delivery API member authentication must be absolute - cannot enable member authentication");
            return false;
        }

        return true;
    }
}
