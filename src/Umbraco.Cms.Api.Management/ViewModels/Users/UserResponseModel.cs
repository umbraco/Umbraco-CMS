﻿using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UserResponseModel : UserPresentationBase, INamedEntityViewModel
{
    public Guid Key { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Language { get; set; }

    public SortedSet<Guid> UserGroups { get; set; } = new();

    public SortedSet<Guid> ContentStartNodeKeys { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeKeys { get; set; } = new();

    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    public UserState State { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? LastlockoutDate { get; set; }

    public DateTime? LastPasswordChangeDate { get; set; }
}
