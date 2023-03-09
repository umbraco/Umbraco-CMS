using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

public class UserInvitationMessage
{
    public required string Message { get; set; }

    public required Uri InviteUri { get; set; }

    public required IUser Recipient { get; set; }

    public required IUser Sender { get; set; }
}
