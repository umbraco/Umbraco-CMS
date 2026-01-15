using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrentUserResponseModel : UserPresentationBase
{
    public required Guid Id { get; init; }

    public required string? LanguageIsoCode { get; init; }

    public required ISet<ReferenceByIdModel> DocumentStartNodeIds { get; init; } = new HashSet<ReferenceByIdModel>();

    public required bool HasDocumentRootAccess { get; init; }

    public required ISet<ReferenceByIdModel> MediaStartNodeIds { get; init; } = new HashSet<ReferenceByIdModel>();

    public required bool HasMediaRootAccess { get; init; }

    public required IEnumerable<string> AvatarUrls { get; init; } = Enumerable.Empty<string>();

    public required IEnumerable<string> Languages { get; init; } = Enumerable.Empty<string>();

    public required bool HasAccessToAllLanguages { get; init; }

    public required bool HasAccessToSensitiveData { get; set; }

    public required ISet<string> FallbackPermissions { get; init; }

    public required ISet<IPermissionPresentationModel> Permissions { get; init; }

    public required ISet<string> AllowedSections { get; init; }

    public bool IsAdmin { get; set; }
}
