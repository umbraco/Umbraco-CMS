using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the message data for a user invitation notification.
/// </summary>
public class UserInvitationMessage
{
    /// <summary>
    ///     Gets or sets the custom message to include in the invitation.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    ///     Gets or sets the URI for the invitation acceptance page.
    /// </summary>
    public required Uri InviteUri { get; set; }

    /// <summary>
    ///     Gets or sets the user who will receive the invitation.
    /// </summary>
    public required IUser Recipient { get; set; }

    /// <summary>
    ///     Gets or sets the user who is sending the invitation.
    /// </summary>
    public required IUser Sender { get; set; }
}
