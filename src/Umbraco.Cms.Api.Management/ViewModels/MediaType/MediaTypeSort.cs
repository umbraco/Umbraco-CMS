namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

/// <summary>
/// Provides sorting options or criteria for organizing media types in the API.
/// </summary>
public class MediaTypeSort
{
    /// <summary>
    /// Gets or sets a reference to the media type by its unique identifier.
    /// </summary>
    public required ReferenceByIdModel MediaType { get; init; }

    /// <summary>
    /// Gets the position of the media type in the sort order.
    /// </summary>
    public int SortOrder { get; init; }
}
