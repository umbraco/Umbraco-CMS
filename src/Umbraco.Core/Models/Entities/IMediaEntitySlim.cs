namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents a lightweight media entity, managed by the entity service.
/// </summary>
public interface IMediaEntitySlim : IContentEntitySlim
{
    /// <summary>
    ///     The media file's path/URL
    /// </summary>
    string? MediaPath { get; }

    /// <summary>
    ///     Gets the content variation flags for the media type (e.g., culture, segment).
    /// </summary>
    ContentVariation Variations { get; }

    /// <summary>
    ///     Gets the culture-specific names for this media entity.
    ///     Empty when the media type does not vary by culture.
    /// </summary>
    IReadOnlyDictionary<string, string> CultureNames { get; }
}
