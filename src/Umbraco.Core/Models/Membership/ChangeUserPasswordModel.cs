namespace Umbraco.Cms.Core.Models.Membership;

public class ChangeUserPasswordModel
{
    public required string NewPassword { get; set; }

    public string? OldPassword { get; set; }

    public string? ResetPasswordToken { get; set; }

    public Guid UserKey { get; set; }
}
