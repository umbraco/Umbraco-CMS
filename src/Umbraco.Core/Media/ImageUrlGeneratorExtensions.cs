using System;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Extensions
{
    public static class ImageUrlGeneratorExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to a supported image.
        /// </summary>
        /// <param name="imageUrlGenerator">The image URL generator implementation that provides detail on which image extension sare supported.</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public static bool IsSupportedImageFormat(this IImageUrlGenerator imageUrlGenerator, string extension)
        {
            if (imageUrlGenerator == null) throw new ArgumentNullException(nameof(imageUrlGenerator));
            if (extension == null) return false;
            extension = extension.TrimStart('.');
            return imageUrlGenerator.SupportedImageFileTypes.InvariantContains(extension);
        }
    }
}
