using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Extensions;

public static class ImageUrlGeneratorExtensions
{
    /// <summary>
    ///     Gets a value indicating whether the file extension corresponds to a supported image.
    /// </summary>
    /// <param name="imageUrlGenerator">
    ///     The image URL generator implementation that provides detail on which image extensions
    ///     are supported.
    /// </param>
    /// <param name="extension">The file extension.</param>
    /// <returns>
    ///     A value indicating whether the file extension corresponds to an image.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">imageUrlGenerator</exception>
    public static bool IsSupportedImageFormat(this IImageUrlGenerator imageUrlGenerator, string extension)
    {
        if (imageUrlGenerator == null)
        {
            throw new ArgumentNullException(nameof(imageUrlGenerator));
        }

        return string.IsNullOrWhiteSpace(extension) == false &&
               imageUrlGenerator.SupportedImageFileTypes.InvariantContains(
                   extension.TrimStart(Constants.CharArrays.Period));
    }
}
