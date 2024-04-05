using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrentUserResponseModel
{
    public required Guid Id { get; init; }

    public required string Email { get; init; } = string.Empty;

    public required string UserName { get; init; } = string.Empty;

    public required string Name { get; init; } = string.Empty;

    public required string? LanguageIsoCode { get; init; }

    public required ISet<Guid> DocumentStartNodeIds { get; init; } = new HashSet<Guid>();

    public required ISet<Guid> MediaStartNodeIds { get; init; } = new HashSet<Guid>();

    public required IEnumerable<string> AvatarUrls { get; init; } = Enumerable.Empty<string>();

    public required IEnumerable<string> Languages { get; init; } = Enumerable.Empty<string>();

    public required bool HasAccessToAllLanguages { get; init; }

    public required bool HasAccessToSensitiveData { get; set; }

    public required ISet<string> FallbackPermissions { get; init; }

    public required ISet<IPermissionPresentationModel> Permissions { get; init; }

    public required ISet<string> AllowedSections { get; init; }
    public bool IsAdmin { get; set; }
}
