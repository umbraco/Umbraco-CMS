using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserResponseModel : UserPresentationBase
{
    public Guid Id { get; set; }

    public string? LanguageIsoCode { get; set; }

    public ISet<Guid> DocumentStartNodeIds { get; set; } = new HashSet<Guid>();

    public ISet<Guid> MediaStartNodeIds { get; set; } = new HashSet<Guid>();

    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    public UserState State { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset UpdateDate { get; set; }

    public DateTimeOffset? LastLoginDate { get; set; }

    public DateTimeOffset? LastLockoutDate { get; set; }

    public DateTimeOffset? LastPasswordChangeDate { get; set; }
    public bool IsAdmin { get; set; }
}
