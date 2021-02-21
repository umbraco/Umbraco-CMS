using Cysharp.Text;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    internal class ImageProcessorImageUrlGenerator : IImageUrlGenerator
    {
        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null) return null;

            using (var imageProcessorUrl = ZString.CreateStringBuilder(true))
            {
                if(options.ImageUrl != null)
                {
                    imageProcessorUrl.Append(options.ImageUrl);
                }

                if (options.FocalPoint != null)
                {
                    imageProcessorUrl.Append("?center=");
                    imageProcessorUrl.Append(options.FocalPoint.Top.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append(",");
                    imageProcessorUrl.Append(options.FocalPoint.Left.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append("&mode=crop");
                }
                else if (options.Crop != null)
                {
                    imageProcessorUrl.Append("?crop=");
                    imageProcessorUrl.Append(options.Crop.X1.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append(",");
                    imageProcessorUrl.Append(options.Crop.Y1.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append(",");
                    imageProcessorUrl.Append(options.Crop.X2.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append(",");
                    imageProcessorUrl.Append(options.Crop.Y2.ToString(CultureInfo.InvariantCulture));
                    imageProcessorUrl.Append("&cropmode=percentage");
                }
                else if (options.DefaultCrop) imageProcessorUrl.Append("?anchor=center&mode=crop");
                else
                {
                    imageProcessorUrl.Append("?mode=");
                    imageProcessorUrl.Append((options.ImageCropMode?.ToLower() ?? "crop"));

                    if (options.ImageCropAnchor != null)
                    {
                        imageProcessorUrl.Append("&anchor=");
                        imageProcessorUrl.Append(options.ImageCropAnchor.ToLower());
                    }
                }

                var hasFormat = options.FurtherOptions != null && options.FurtherOptions.InvariantContains("&format=");

                //Only put quality here, if we don't have a format specified.
                //Otherwise we need to put quality at the end to avoid it being overridden by the format.
                if (options.Quality != null && hasFormat == false)
                {
                    imageProcessorUrl.Append("&quality=");
                    imageProcessorUrl.Append(options.Quality);
                }
                if (options.HeightRatio != null) {
                    imageProcessorUrl.Append("&heightratio=");
                    imageProcessorUrl.Append(options.HeightRatio.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (options.WidthRatio != null) {
                    imageProcessorUrl.Append("&widthratio=");
                    imageProcessorUrl.Append(options.WidthRatio.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (options.Width != null) {
                    imageProcessorUrl.Append("&width=");
                    imageProcessorUrl.Append(options.Width);
                }
                if (options.Height != null) {
                    imageProcessorUrl.Append("&height=");
                    imageProcessorUrl.Append(options.Height);
                }
                if (options.UpScale == false) imageProcessorUrl.Append("&upscale=false");
                if (options.AnimationProcessMode != null) {
                    imageProcessorUrl.Append("&animationprocessmode=");
                    imageProcessorUrl.Append(options.AnimationProcessMode);
                }
                if (options.FurtherOptions != null) imageProcessorUrl.Append(options.FurtherOptions);

                //If furtherOptions contains a format, we need to put the quality after the format.
                if (options.Quality != null && hasFormat) {
                    imageProcessorUrl.Append("&quality=");
                    imageProcessorUrl.Append(options.Quality);
                }
                if (options.CacheBusterValue != null) {
                    imageProcessorUrl.Append("&rnd=");
                    imageProcessorUrl.Append(options.CacheBusterValue);
                }

                return imageProcessorUrl.ToString();
            }
        }
    }
}
