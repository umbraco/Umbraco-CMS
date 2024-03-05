using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Handlers;

internal sealed class RevokeMemberAuthenticationTokensNotificationHandler
    : INotificationAsyncHandler<MemberSavedNotification>,
        INotificationAsyncHandler<MemberDeletedNotification>,
        INotificationAsyncHandler<AssignedMemberRolesNotification>,
        INotificationAsyncHandler<RemovedMemberRolesNotification>
{
    private readonly IMemberService _memberService;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly bool _enabled;
    private readonly ILogger<RevokeMemberAuthenticationTokensNotificationHandler> _logger;

    public RevokeMemberAuthenticationTokensNotificationHandler(
        IMemberService memberService,
        IOpenIddictTokenManager tokenManager,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<RevokeMemberAuthenticationTokensNotificationHandler> logger)
    {
        _memberService = memberService;
        _tokenManager = tokenManager;
        _logger = logger;
        _enabled = deliveryApiSettings.Value.MemberAuthorizationIsEnabled();
    }

    public async Task HandleAsync(MemberSavedNotification notification, CancellationToken cancellationToken)
    {
        if (_enabled is false)
        {
            return;
        }

        foreach (IMember member in notification.SavedEntities.Where(member => member.IsLockedOut || member.IsApproved is false))
        {
            // member is locked out and/or un-approved, make sure we revoke all tokens
            await RevokeTokensAsync(member);
        }
    }

    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken)
    {
        if (_enabled is false)
        {
            return;
        }

        foreach (IMember member in notification.DeletedEntities)
        {
            await RevokeTokensAsync(member);
        }
    }

    public async Task HandleAsync(AssignedMemberRolesNotification notification, CancellationToken cancellationToken)
        => await MemberRolesChangedAsync(notification);

    public async Task HandleAsync(RemovedMemberRolesNotification notification, CancellationToken cancellationToken)
        => await MemberRolesChangedAsync(notification);

    private async Task RevokeTokensAsync(IMember member)
    {
        var tokens = await _tokenManager.FindBySubjectAsync(member.Key.ToString()).ToArrayAsync();
        if (tokens.Any() is false)
        {
            return;
        }

        _logger.LogInformation("Deleting {count} active tokens for member with ID {id}", tokens.Length, member.Id);
        foreach (var token in tokens)
        {
            await _tokenManager.DeleteAsync(token);
        }
    }

    private async Task MemberRolesChangedAsync(MemberRolesNotification notification)
    {
        if (_enabled is false)
        {
            return;
        }

        foreach (var memberId in notification.MemberIds)
        {
            IMember? member = _memberService.GetById(memberId);
            if (member is null)
            {
                _logger.LogWarning("Unable to find member with ID {id}", memberId);
                continue;
            }

            await RevokeTokensAsync(member);
        }
    }
}
