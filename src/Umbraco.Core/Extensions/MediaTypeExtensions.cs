// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IMediaType"/>.
/// </summary>
public static class MediaTypeExtensions
{
    /// <summary>
    /// Determines whether the media type is a built-in system media type (File, Folder, or Image).
    /// </summary>
    /// <param name="mediaType">The media type to check.</param>
    /// <returns><c>true</c> if the media type is a system media type; otherwise, <c>false</c>.</returns>
    public static bool IsSystemMediaType(this IMediaType mediaType) =>
        mediaType.Alias == Constants.Conventions.MediaTypes.File
        || mediaType.Alias == Constants.Conventions.MediaTypes.Folder
        || mediaType.Alias == Constants.Conventions.MediaTypes.Image;
}
