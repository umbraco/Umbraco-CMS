using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Api.Delivery.Handlers;

internal sealed class InitializeMemberApplicationNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<InitializeMemberApplicationNotificationHandler> _logger;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMemberClientCredentialsManager _memberClientCredentialsManager;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    private static readonly SemaphoreSlim _locker = new(1);
    private static bool _isInitialized = false;

    public InitializeMemberApplicationNotificationHandler(
        IRuntimeState runtimeState,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<InitializeMemberApplicationNotificationHandler> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMemberClientCredentialsManager memberClientCredentialsManager,
        IServerRoleAccessor serverRoleAccessor)
    {
        _runtimeState = runtimeState;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _memberClientCredentialsManager = memberClientCredentialsManager;
        _serverRoleAccessor = serverRoleAccessor;
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        if (_serverRoleAccessor.CurrentServerRole is ServerRole.Subscriber)
        {
            // subscriber instances should not alter the member application
            return;
        }

        try
        {
            await _locker.WaitAsync(cancellationToken);
            if (_isInitialized)
            {
                return;
            }

            // we cannot inject the IMemberApplicationManager because it ultimately takes a dependency on the DbContext ... and during
            // install that is not allowed (no connection string means no DbContext)
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IMemberApplicationManager memberApplicationManager = scope.ServiceProvider.GetRequiredService<IMemberApplicationManager>();

            await HandleMemberApplication(memberApplicationManager, cancellationToken);
            await HandleMemberClientCredentialsApplication(memberApplicationManager, cancellationToken);

            _isInitialized = true;
        }
        finally
        {
            _locker.Release();
        }
    }

    private async Task HandleMemberApplication(IMemberApplicationManager memberApplicationManager, CancellationToken cancellationToken)
    {
        if (_deliveryApiSettings.MemberAuthorization?.AuthorizationCodeFlow?.Enabled is not true)
        {
            await memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        if (ValidateRedirectUrls(_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LoginRedirectUrls) is false)
        {
            await memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        if (_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls.Any()
            && ValidateRedirectUrls(_deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls) is false)
        {
            await memberApplicationManager.DeleteMemberApplicationAsync(cancellationToken);
            return;
        }

        await memberApplicationManager.EnsureMemberApplicationAsync(
            _deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LoginRedirectUrls,
            _deliveryApiSettings.MemberAuthorization.AuthorizationCodeFlow.LogoutRedirectUrls,
            cancellationToken);
    }

    private async Task HandleMemberClientCredentialsApplication(IMemberApplicationManager memberApplicationManager, CancellationToken cancellationToken)
    {
        if (_deliveryApiSettings.MemberAuthorization?.ClientCredentialsFlow?.Enabled is not true)
        {
            // disabled
            return;
        }

        IEnumerable<MemberClientCredentials> memberClientCredentials = await _memberClientCredentialsManager.GetAllAsync();
        foreach (MemberClientCredentials memberClientCredential in memberClientCredentials)
        {
            await memberApplicationManager.EnsureMemberClientCredentialsApplicationAsync(memberClientCredential.ClientId, memberClientCredential.ClientSecret, cancellationToken);
        }
    }

    private bool ValidateRedirectUrls(IEnumerable<Uri> redirectUrls)
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
