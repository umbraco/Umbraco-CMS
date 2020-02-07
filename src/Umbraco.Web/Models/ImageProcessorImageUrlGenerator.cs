using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    internal class ImageProcessorImageUrlGenerator : IImageUrlGenerator
    {
        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            var imageProcessorUrl = new StringBuilder(options.ImageUrl ?? string.Empty);

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
                imageProcessorUrl.Append(options.Crop.X1.ToString(CultureInfo.InvariantCulture)).Append(",");
                imageProcessorUrl.Append(options.Crop.Y1.ToString(CultureInfo.InvariantCulture)).Append(",");
                imageProcessorUrl.Append(options.Crop.X2.ToString(CultureInfo.InvariantCulture)).Append(",");
                imageProcessorUrl.Append(options.Crop.Y2.ToString(CultureInfo.InvariantCulture));
                imageProcessorUrl.Append("&cropmode=percentage");
            }
            else if (options.DefaultCrop)
            {
                imageProcessorUrl.Append("?anchor=center");
                imageProcessorUrl.Append("&mode=crop");
            }
            else
            {
                imageProcessorUrl.Append("?mode=" + options.ImageCropMode.ToString().ToLower());

                if (options.ImageCropAnchor != null)
                {
                    imageProcessorUrl.Append("&anchor=" + options.ImageCropAnchor.ToString().ToLower());
                }
            }

            var hasFormat = options.FurtherOptions != null && options.FurtherOptions.InvariantContains("&format=");

            //Only put quality here, if we don't have a format specified.
            //Otherwise we need to put quality at the end to avoid it being overridden by the format.
            if (options.Quality != null && hasFormat == false)
            {
                imageProcessorUrl.Append("&quality=" + options.Quality);
            }

            if (options.HeightRatio != null)
            {
                imageProcessorUrl.Append("&heightratio=" + options.HeightRatio.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (options.WidthRatio != null)
            {
                imageProcessorUrl.Append("&widthratio=" + options.WidthRatio.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (options.Width != null)
            {
                imageProcessorUrl.Append("&width=" + options.Width);
            }

            if (options.Height != null)
            {
                imageProcessorUrl.Append("&height=" + options.Height);
            }

            if (options.UpScale == false)
            {
                imageProcessorUrl.Append("&upscale=false");
            }

            if (options.AnimationProcessMode != null)
            {
                imageProcessorUrl.Append("&animationprocessmode=" + options.AnimationProcessMode);
            }

            if (options.FurtherOptions != null)
            {
                imageProcessorUrl.Append(options.FurtherOptions);
            }

            //If furtherOptions contains a format, we need to put the quality after the format.
            if (options.Quality != null && hasFormat)
            {
                imageProcessorUrl.Append("&quality=" + options.Quality);
            }

            if (options.CacheBusterValue != null)
            {
                imageProcessorUrl.Append("&rnd=").Append(options.CacheBusterValue);
            }

            return imageProcessorUrl.ToString();
        }
    }
}
