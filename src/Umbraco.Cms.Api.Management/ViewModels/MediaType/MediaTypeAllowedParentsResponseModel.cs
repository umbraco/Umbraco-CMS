namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents a response model containing the list of allowed parent media types for a specific media type.
/// </summary>
public class MediaTypeAllowedParentsResponseModel
{
    /// <summary>
    /// Gets or sets the set of allowed parent media type references, identified by their IDs, for this media type.
    /// </summary>
    public required ISet<ReferenceByIdModel> AllowedParentIds { get; set; }
}
