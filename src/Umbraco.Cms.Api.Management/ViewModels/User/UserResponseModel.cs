﻿using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserResponseModel : UserPresentationBase, INamedEntityPresentationModel
{
    public Guid Id { get; set; }

    public string? LanguageIsoCode { get; set; }

    public SortedSet<Guid> ContentStartNodeIds { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeIds { get; set; } = new();

    public IEnumerable<string> AvatarUrls { get; set; } = Enumerable.Empty<string>();

    public UserState State { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? LastlockoutDate { get; set; }

    public DateTime? LastPasswordChangeDate { get; set; }
}
