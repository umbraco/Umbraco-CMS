namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the API response model containing the calculated start nodes (such as content or media) for a user.
/// </summary>
public class CalculatedUserStartNodesResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the calculated user start node.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the set of document start node IDs.
    /// </summary>
    public ISet<ReferenceByIdModel> DocumentStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the user has access to the document root.
    /// </summary>
    public bool HasDocumentRootAccess { get; set; }

    /// <summary>
    /// Gets or sets the set of calculated media start node IDs for the user.
    /// </summary>
    public ISet<ReferenceByIdModel> MediaStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the user has access to the media root.
    /// </summary>
    public bool HasMediaRootAccess { get; set; }
}
