using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions;

public static class MediaExtensions
{
    /// <summary>
    ///     Gets the URL of a media item.
    /// </summary>
    public static string? GetUrl(this IMedia media, string propertyAlias, MediaUrlGeneratorCollection mediaUrlGenerators)
    {
        if (media.TryGetMediaPath(propertyAlias, mediaUrlGenerators, out var mediaPath))
        {
            return mediaPath;
        }

        return string.Empty;
    }

    /// <summary>
    ///     Gets the URLs of a media item.
    /// </summary>
    public static string?[] GetUrls(this IMedia media, ContentSettings contentSettings, MediaUrlGeneratorCollection mediaUrlGenerators)
        => contentSettings.Imaging.AutoFillImageProperties
            .Select(field => media.GetUrl(field.Alias, mediaUrlGenerators))
            .Where(link => string.IsNullOrWhiteSpace(link) == false)
            .ToArray();
}
