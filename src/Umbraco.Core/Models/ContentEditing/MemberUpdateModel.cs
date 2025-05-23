namespace Umbraco.Cms.Core.Models.ContentEditing;

public class MemberUpdateModel : MemberEditingModelBase
{
    public bool IsLockedOut { get; set; }

    public bool IsTwoFactorEnabled { get; set; }

    public string? OldPassword { get; set; }

    public string? NewPassword { get; set; }
}
