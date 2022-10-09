using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
///     Deletes the external logins for the deleted members. This cannot be handled by the database as there is not foreign
///     keys.
/// </summary>
public class DeleteExternalLoginsOnMemberDeletedHandler : INotificationHandler<MemberDeletedNotification>
{
    private readonly IExternalLoginWithKeyService _externalLoginWithKeyService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteExternalLoginsOnMemberDeletingHandler" /> class.
    /// </summary>
    public DeleteExternalLoginsOnMemberDeletedHandler(IExternalLoginWithKeyService externalLoginWithKeyService)
        => _externalLoginWithKeyService = externalLoginWithKeyService;

    /// <inheritdoc />
    public void Handle(MemberDeletedNotification notification)
    {
        foreach (IMember member in notification.DeletedEntities)
        {
            _externalLoginWithKeyService.DeleteUserLogins(member.Key);
        }
    }
}
