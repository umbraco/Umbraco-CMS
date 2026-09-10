using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for reading the file extension of a media item.
/// </summary>
/// <remarks>
/// The two overloads read different sources because the two models carry different data: a slim entity knows the
/// path of the stored file, while a full media item carries the <c>umbracoExtension</c> property. Both are
/// normalised the same way so every read model reports the extension in the same shape.
/// </remarks>
public static class MediaFileExtensions
{
    /// <summary>
    /// Gets the file extension of a media entity, without the leading dot and in lowercase.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The file extension (e.g. "jpg"), or <c>null</c> when the entity holds no file, such as a folder.</returns>
    public static string? GetFileExtension(this IMediaEntitySlim entity)
        => Normalize(entity.MediaPath?.GetFileExtension());

    /// <summary>
    /// Gets the file extension of a media item, without the leading dot and in lowercase.
    /// </summary>
    /// <param name="media">The media item.</param>
    /// <returns>The file extension (e.g. "jpg"), or <c>null</c> when the item holds no file, such as a folder.</returns>
    public static string? GetFileExtension(this IMedia media)
        => Normalize(media.GetValue<string>(Constants.Conventions.Media.Extension));

    private static string? Normalize(string? extension)
    {
        extension = extension?.TrimStart(Constants.CharArrays.Period).ToLowerInvariant();

        return string.IsNullOrEmpty(extension) ? null : extension;
    }
}
