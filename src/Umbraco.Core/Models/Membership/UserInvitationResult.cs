namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the result of a user invitation operation.
/// </summary>
public class UserInvitationResult: ErrorMessageResult
{
    /// <summary>
    ///     Gets or initializes the invited user, if the operation succeeded.
    /// </summary>
    public IUser? InvitedUser { get; init; }
}
