using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
///     Deletes the two factor for the deleted members. This cannot be handled by the database as there is not foreign
///     keys.
/// </summary>
public class DeleteTwoFactorLoginsOnMemberDeletedHandler : INotificationAsyncHandler<MemberDeletedNotification>
{
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteTwoFactorLoginsOnMemberDeletedHandler" /> class.
    /// </summary>
    public DeleteTwoFactorLoginsOnMemberDeletedHandler(ITwoFactorLoginService twoFactorLoginService)
        => _twoFactorLoginService = twoFactorLoginService;

    /// <inheritdoc />
    public async Task HandleAsync(MemberDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMember member in notification.DeletedEntities)
        {
            await _twoFactorLoginService.DeleteUserLoginsAsync(member.Key);
        }
    }
}
