using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Media;

/// <summary>
///     Exposes a method that generates an image URL based on the specified options.
/// </summary>
public interface IImageUrlGenerator
{
    /// <summary>
    ///     Gets the supported image file types/extensions.
    /// </summary>
    /// <value>
    ///     The supported image file types/extensions.
    /// </value>
    IEnumerable<string> SupportedImageFileTypes { get; }

    /// <summary>
    ///     Gets the image URL based on the specified <paramref name="options" />.
    /// </summary>
    /// <param name="options">The image URL generation options.</param>
    /// <returns>
    ///     The generated image URL.
    /// </returns>
    string? GetImageUrl(ImageUrlGenerationOptions options);
}
