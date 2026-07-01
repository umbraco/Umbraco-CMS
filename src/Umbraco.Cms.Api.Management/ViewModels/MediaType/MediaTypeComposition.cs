using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Represents a media type composition in the Umbraco CMS Management API.
/// </summary>
public class MediaTypeComposition
{
    /// <summary>
    /// Gets or sets a reference to the media type by its ID.
    /// </summary>
    public required ReferenceByIdModel MediaType { get; init; }

    /// <summary>
    /// Gets or sets the type of composition applied to the media type, indicating how it inherits or combines properties from other types.
    /// </summary>
    public required CompositionType CompositionType { get; init; }
}
