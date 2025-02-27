using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Security;

public interface IUserInviteSender
{
    Task InviteUser(UserInvitationMessage invite);

    bool CanSendInvites();
}
