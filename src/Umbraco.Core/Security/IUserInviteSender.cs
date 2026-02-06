using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to send user invitation messages.
/// </summary>
public interface IUserInviteSender
{
    /// <summary>
    ///     Sends an invitation to the user.
    /// </summary>
    /// <param name="invite">The invitation message containing user and invite details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InviteUser(UserInvitationMessage invite);

    /// <summary>
    ///     Determines whether the sender is configured and able to send invites.
    /// </summary>
    /// <returns><c>true</c> if the sender can send invites; otherwise, <c>false</c>.</returns>
    bool CanSendInvites();
}
