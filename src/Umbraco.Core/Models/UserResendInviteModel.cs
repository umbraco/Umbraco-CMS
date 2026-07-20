namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the model used for resending an invitation to a user.
/// </summary>
public class UserResendInviteModel
{
    /// <summary>
    ///     Gets or sets an optional custom message to include in the resent invitation email.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of the user whose invitation should be resent.
    /// </summary>
    public Guid InvitedUserKey { get; set; }
}
