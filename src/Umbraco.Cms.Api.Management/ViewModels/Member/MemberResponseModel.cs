using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class MemberResponseModel : ContentResponseModelBase<MemberValueModel, MemberVariantResponseModel>
{
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public MemberTypeReferenceResponseModel MemberType { get; set; } = new();

    public bool IsApproved { get; set; }

    public bool IsLockedOut { get; set; }

    public bool IsTwoFactorEnabled { get; set; }

    public int FailedPasswordAttempts { get; set; }

    public DateTimeOffset? LastLoginDate { get; set; }

    public DateTimeOffset? LastLockoutDate { get; set; }

    public DateTimeOffset? LastPasswordChangeDate { get; set; }

    public IEnumerable<Guid> Groups { get; set; } = [];
}
