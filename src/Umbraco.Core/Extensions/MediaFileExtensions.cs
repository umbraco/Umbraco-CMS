using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for reading the file extension of a media item. Both overloads read the stored file; a slim
/// entity already carries its path, a full media item resolves it.
/// </summary>
public static class MediaFileExtensions
{
    /// <summary>
    /// Gets the file extension of a media entity, without the leading dot and in lowercase.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The file extension, or <c>null</c> when there is no file.</returns>
    public static string? GetFileExtension(this IMediaEntitySlim entity)
        => Normalize(entity.MediaPath?.GetFileExtension());

    /// <summary>
    /// Gets the file extension of a media item, without the leading dot and in lowercase.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <param name="mediaUrlGenerators">Used to resolve the stored file's path.</param>
    /// <returns>The file extension, or <c>null</c> when there is no file.</returns>
    public static string? GetFileExtension(this IMedia media, MediaUrlGeneratorCollection mediaUrlGenerators)
        => media.TryGetMediaPath(Constants.Conventions.Media.File, mediaUrlGenerators, out var mediaPath)
            ? Normalize(mediaPath?.GetFileExtension())
            : null;

    private static string? Normalize(string? extension)
    {
        extension = extension?.TrimStart(Constants.CharArrays.Period).ToLowerInvariant();

        return string.IsNullOrEmpty(extension) ? null : extension;
    }
}
