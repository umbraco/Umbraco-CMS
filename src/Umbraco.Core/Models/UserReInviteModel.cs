namespace Umbraco.Cms.Core.Models;

public class UserResendInviteModel
{
    public string? Message { get; set; }
    public Guid InvitedUserKey { get; set; }
}
