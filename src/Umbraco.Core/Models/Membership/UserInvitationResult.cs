namespace Umbraco.Cms.Core.Models.Membership;

public class UserInvitationResult: ErrorMessageResult
{
    public IUser? InvitedUser { get; init; }
}
