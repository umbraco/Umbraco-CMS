using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Media
{
    public class ImageSharpImageUrlGenerator : IImageUrlGenerator
    {
        public IEnumerable<string> SupportedImageFileTypes => new[]
        {
            "jpeg",
            "jpg",
            "gif",
            "bmp",
            "png"
        };

        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null)
            {
                return null;
            }

            var imageUrl = new StringBuilder(options.ImageUrl);

            if (options.FocalPoint != null)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "?rxy={0},{1}", options.FocalPoint.Top, options.FocalPoint.Left);
            }
            else if (options.Crop != null)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "?crop={0},{1},{2},{3}&cropmode=percentage", options.Crop.X1, options.Crop.Y1, options.Crop.X2, options.Crop.Y2);
            }
            else if (options.DefaultCrop)
            {
                imageUrl.Append("?anchor=center&mode=crop");
            }
            else
            {
                imageUrl.Append("?mode=").Append((options.ImageCropMode ?? ImageCropMode.Crop).ToString().ToLowerInvariant());

                if (options.ImageCropAnchor != null)
                {
                    imageUrl.Append("&anchor=").Append(options.ImageCropAnchor.ToString().ToLowerInvariant());
                }
            }

            var hasFormat = options.FurtherOptions != null && options.FurtherOptions.InvariantContains("&format=");

            // Only put quality here, if we don't have a format specified.
            // Otherwise we need to put quality at the end to avoid it being overridden by the format.
            if (options.Quality.HasValue && hasFormat == false)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&quality={0}", options.Quality.Value);
            }

            if (options.HeightRatio.HasValue)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&heightratio={0}", options.HeightRatio.Value);
            }

            if (options.WidthRatio.HasValue)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&widthratio={0}", options.WidthRatio.Value);
            }

            if (options.Width.HasValue)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&width={0}", options.Width.Value);
            }

            if (options.Height.HasValue)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&height={0}", options.Height.Value);
            }

            if (options.UpScale == false)
            {
                imageUrl.Append("&upscale=false");
            }

            if (!string.IsNullOrWhiteSpace(options.AnimationProcessMode))
            {
                imageUrl.Append("&animationprocessmode=").Append(options.AnimationProcessMode);
            }

            if (!string.IsNullOrWhiteSpace(options.FurtherOptions))
            {
                imageUrl.Append(options.FurtherOptions);
            }

            // If furtherOptions contains a format, we need to put the quality after the format.
            if (options.Quality.HasValue && hasFormat)
            {
                imageUrl.AppendFormat(CultureInfo.InvariantCulture, "&quality={0}", options.Quality.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.CacheBusterValue))
            {
                imageUrl.Append("&rnd=").Append(options.CacheBusterValue);
            }

            return imageUrl.ToString();
        }
    }
}
