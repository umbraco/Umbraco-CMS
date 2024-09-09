using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class UpdateMemberRequestModel : UpdateContentRequestModelBase<MemberValueModel, MemberVariantRequestModel>
{
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? OldPassword { get; set; }

    public string? NewPassword { get; set; }

    public IEnumerable<Guid>? Groups { get; set; }

    public bool IsApproved { get; set; }

    public bool IsLockedOut { get; set; }

    public bool IsTwoFactorEnabled { get; set; }
}
