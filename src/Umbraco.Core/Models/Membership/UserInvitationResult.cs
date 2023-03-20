namespace Umbraco.Cms.Core.Models.Membership;

public class UserInvitationResult: IErrorMessageResult
{
    public IUser? InvitedUser { get; init; }

    public string? ErrorMessage { get; init; }
}
