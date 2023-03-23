using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Extensions;

public static class ImageDimensionExtractorExtensions
{
    /// <summary>
    /// Gets a value indicating whether the file extension corresponds to a supported image.
    /// </summary>
    /// <param name="imageDimensionExtractor">The image dimension extractor implementation that provides detail on which image extensions are supported.</param>
    /// <param name="extension">The file extension.</param>
    /// <returns>
    /// A value indicating whether the file extension corresponds to an image.
    /// </returns>
    public static bool IsSupportedImageFormat(this IImageDimensionExtractor imageDimensionExtractor, string extension)
    {
        ArgumentNullException.ThrowIfNull(imageDimensionExtractor);

        return string.IsNullOrWhiteSpace(extension) == false &&
            imageDimensionExtractor.SupportedImageFileTypes.InvariantContains(extension.TrimStart(Constants.CharArrays.Period));
    }
}
