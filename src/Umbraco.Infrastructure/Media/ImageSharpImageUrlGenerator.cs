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
        public IEnumerable<string> SupportedImageFileTypes => new[] { "jpeg", "jpg", "gif", "bmp", "png" };

        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null) return null;

            var imageSharpWebUrl = new StringBuilder(options.ImageUrl ?? string.Empty);

            if (options.FocalPoint != null) AppendFocalPoint(imageSharpWebUrl, options);
            else if (options.Crop != null) AppendCrop(imageSharpWebUrl, options);
            else if (options.DefaultCrop) imageSharpWebUrl.Append("?ranchor=center&rmode=crop");
            else
            {
                imageSharpWebUrl.Append("?rmode=").Append((options.ImageCropMode ?? ImageCropMode.Crop).ToString().ToLower());

                if (options.ImageCropAnchor != null) imageSharpWebUrl.Append("&ranchor=").Append(options.ImageCropAnchor.ToString().ToLower());
            }

            var hasFormat = options.FurtherOptions != null && options.FurtherOptions.InvariantContains("&format=");

            //Only put quality here, if we don't have a format specified.
            //Otherwise we need to put quality at the end to avoid it being overridden by the format.
            if (options.Quality.HasValue && hasFormat == false) imageSharpWebUrl.Append("&quality=").Append(options.Quality);

            // ** TO DO **
            if (options.HeightRatio.HasValue) imageSharpWebUrl.Append("&heightratio=").Append(options.HeightRatio.Value.ToString(CultureInfo.InvariantCulture));
            // ** TO DO **
            if (options.WidthRatio.HasValue) imageSharpWebUrl.Append("&widthratio=").Append(options.WidthRatio.Value.ToString(CultureInfo.InvariantCulture));

            if (options.Width.HasValue) imageSharpWebUrl.Append("&width=").Append(options.Width);
            if (options.Height.HasValue) imageSharpWebUrl.Append("&height=").Append(options.Height);
            if (options.UpScale == false) imageSharpWebUrl.Append("&upscale=false");
            if (!string.IsNullOrWhiteSpace(options.AnimationProcessMode)) imageSharpWebUrl.Append("&animationprocessmode=").Append(options.AnimationProcessMode);
            if (!string.IsNullOrWhiteSpace(options.FurtherOptions)) imageSharpWebUrl.Append(options.FurtherOptions);

            //If furtherOptions contains a format, we need to put the quality after the format.
            if (options.Quality.HasValue && hasFormat) imageSharpWebUrl.Append("&quality=").Append(options.Quality);
            if (!string.IsNullOrWhiteSpace(options.CacheBusterValue)) imageSharpWebUrl.Append("&rnd=").Append(options.CacheBusterValue);

            return imageSharpWebUrl.ToString();
        }

        private void AppendFocalPoint(StringBuilder imageProcessorUrl, ImageUrlGenerationOptions options)
        {
            imageProcessorUrl.Append("?rxy=");
            imageProcessorUrl.Append(options.FocalPoint.Top.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.FocalPoint.Left.ToString(CultureInfo.InvariantCulture));
            imageProcessorUrl.Append("&rmode=crop");
        }

        private void AppendCrop(StringBuilder imageProcessorUrl, ImageUrlGenerationOptions options)
        {
            // ** TO DO **
            imageProcessorUrl.Append("?crop=");
            imageProcessorUrl.Append(options.Crop.X1.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.Y1.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.X2.ToString(CultureInfo.InvariantCulture)).Append(",");
            imageProcessorUrl.Append(options.Crop.Y2.ToString(CultureInfo.InvariantCulture));
            imageProcessorUrl.Append("&cropmode=percentage");
        }
    }
}
