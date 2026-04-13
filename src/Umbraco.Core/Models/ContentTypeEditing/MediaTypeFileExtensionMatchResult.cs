using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the result of matching a media type to a file extension, including
///     whether the match was by a specific extension or as a catch-all fallback.
/// </summary>
public class MediaTypeFileExtensionMatchResult
{
    /// <summary>
    ///     Gets the matched media type.
    /// </summary>
    public required IMediaType MediaType { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the media type matched by an explicit file extension
    ///     (e.g., Article for .pdf) rather than as a catch-all fallback (e.g., File with no extension restrictions).
    /// </summary>
    public required bool IsSpecificMatch { get; init; }
}
