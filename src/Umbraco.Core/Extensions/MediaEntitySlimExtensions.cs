using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IMediaEntitySlim" />.
/// </summary>
public static class MediaEntitySlimExtensions
{
    /// <summary>
    /// Gets the file extension of a media entity, without the leading dot and in lowercase.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The file extension (e.g. "jpg"), or <c>null</c> when the entity holds no file, such as a folder.</returns>
    public static string? GetFileExtension(this IMediaEntitySlim entity)
    {
        var extension = entity.MediaPath?.GetFileExtension().TrimStart(Constants.CharArrays.Period).ToLowerInvariant();

        return string.IsNullOrEmpty(extension) ? null : extension;
    }
}
