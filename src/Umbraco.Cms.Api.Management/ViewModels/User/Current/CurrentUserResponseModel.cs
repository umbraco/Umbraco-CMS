using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents the response model containing information about the currently authenticated user.
/// </summary>
public class CurrentUserResponseModel : UserPresentationBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the current user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>Gets or sets the ISO code of the user's language.</summary>
    public required string? LanguageIsoCode { get; init; }

    /// <summary>
    /// Gets or sets the set of content start node IDs (as <see cref="ReferenceByIdModel"/>) associated with the current user.
    /// These define the root nodes in the content tree that the user has access to.
    /// </summary>
    public required ISet<ReferenceByIdModel> DocumentStartNodeIds { get; init; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the current user has access to the document root.
    /// </summary>
    public required bool HasDocumentRootAccess { get; init; }

    /// <summary>
    /// Gets or sets the collection of media start node identifiers (<see cref="ReferenceByIdModel"/>) that define the root media nodes accessible to the current user.
    /// </summary>
    public required ISet<ReferenceByIdModel> MediaStartNodeIds { get; init; } = new HashSet<ReferenceByIdModel>();

    /// <summary>Gets or sets a value indicating whether the current user has access to the media root.</summary>
    public required bool HasMediaRootAccess { get; init; }

    /// <summary>
    /// Gets or sets the collection of URLs representing the current user's avatars, which may include multiple sizes or formats.
    /// </summary>
    public required IEnumerable<string> AvatarUrls { get; init; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets the list of language codes associated with the current user.
    /// Each language is represented by its culture code (e.g., "en-US").
    /// </summary>
    public required IEnumerable<string> Languages { get; init; } = Enumerable.Empty<string>();

    /// <summary>Gets or sets a value indicating whether the current user has access to all languages.</summary>
    public required bool HasAccessToAllLanguages { get; init; }

    /// <summary>Indicates whether the current user has access to sensitive data.</summary>
    public required bool HasAccessToSensitiveData { get; set; }

    /// <summary>
    /// Gets or sets the set of permissions that apply to the current user when no explicit permissions are defined for a specific context.
    /// These fallback permissions serve as the default access rights for the user.
    /// </summary>
    public required ISet<string> FallbackPermissions { get; init; }

    /// <summary>
    /// Gets or sets the collection of permissions assigned to the current user.
    /// Each permission defines an action or access right granted to the user.
    /// </summary>
    public required ISet<IPermissionPresentationModel> Permissions { get; init; }

    /// <summary>
    /// Gets or sets the set of sections the current user is allowed to access.
    /// </summary>
    public required ISet<string> AllowedSections { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user has administrative privileges.
    /// </summary>
    public bool IsAdmin { get; set; }
}
