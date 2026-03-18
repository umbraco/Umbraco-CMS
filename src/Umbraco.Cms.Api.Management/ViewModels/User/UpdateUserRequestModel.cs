namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model for updating information about a user.
/// </summary>
public class UpdateUserRequestModel : UserPresentationBase
{
    /// <summary>
    /// Gets or sets the ISO code of the user's language.
    /// </summary>
    public string LanguageIsoCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the set of content start node IDs for documents that the user has access to.
    /// </summary>
    public ISet<ReferenceByIdModel> DocumentStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Indicates whether the user has access to the document root.
    /// </summary>
    public bool HasDocumentRootAccess { get; init; }

    /// <summary>
    /// Gets or sets the collection of media start node IDs that define the root media nodes accessible to the user.
    /// </summary>
    public ISet<ReferenceByIdModel> MediaStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets a value indicating whether the user has access to the media root.
    /// </summary>
    public bool HasMediaRootAccess { get; init; }
}
