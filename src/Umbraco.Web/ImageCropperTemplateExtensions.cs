namespace Umbraco.Web
{
    using System.Globalization;
    using System.Text;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Web.Models;

    /// <summary>
    /// Provides extension methods for getting ImageProcessor Url from the core Image Cropper property editor
    /// </summary>
    public static class ImageCropperTemplateExtensions
    {
        /// <summary>
        /// Gets the ImageProcessor Url by the crop alias (from the "umbracoFile" property alias) on the IPublishedContent item
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web Url.
        /// </returns>
        public static string GetCropUrl(this IPublishedContent mediaItem, string cropAlias)
        {
            return mediaItem.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true);
        }

        /// <summary>
        /// Gets the ImageProcessor Url by the crop alias using the specified property containing the image cropper Json data on the IPublishedContent item.
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="propertyAlias">
        /// The property alias of the property containing the Json data e.g. umbracoFile
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias e.g. thumbnail
        /// </param>
        /// <returns>
        /// The ImageProcessor.Web Url.
        /// </returns>
        public static string GetCropUrl(this IPublishedContent mediaItem, string propertyAlias, string cropAlias)
        {
            return mediaItem.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);
        }

        /// <summary>
        /// Gets the ImageProcessor Url from the IPublishedContent item.
        /// </summary>
        /// <param name="mediaItem">
        /// The IPublishedContent item.
        /// </param>
        /// <param name="width">
        /// The width of the output image.
        /// </param>
        /// <param name="height">
        /// The height of the output image.
        /// </param>
        /// <param name="propertyAlias">
        /// Property alias of the property containing the Json data.
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias.
        /// </param>
        /// <param name="quality">
        /// Quality percentage of the output image.
        /// </param>
        /// <param name="imageCropMode">
        /// The image crop mode.
        /// </param>
        /// <param name="imageCropAnchor">
        /// The image crop anchor.
        /// </param>
        /// <param name="preferFocalPoint">
        /// Use focal point, to generate an output image using the focal point instead of the predefined crop
        /// </param>
        /// <param name="useCropDimensions">
        /// Use crop dimensions to have the output image sized according to the predefined crop sizes, this will override the width and height parameters>.
        /// </param>
        /// <param name="cacheBuster">
        /// Add a serialised date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// The further options.
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>  
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>         
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
             this IPublishedContent mediaItem,
             int? width = null,
             int? height = null,
             string propertyAlias = Constants.Conventions.Media.File,
             string cropAlias = null,
             int? quality = null,
             ImageCropMode? imageCropMode = null,
             ImageCropAnchor? imageCropAnchor = null,
             bool preferFocalPoint = false,
             bool useCropDimensions = false,
             bool cacheBuster = true, 
             string furtherOptions = null,
             ImageCropRatioMode? ratioMode = null,
             bool upScale = true)
        {
            string imageCropperValue = null;

            string mediaItemUrl;

            if (mediaItem.HasProperty(propertyAlias) && mediaItem.HasValue(propertyAlias))
            {
                imageCropperValue = mediaItem.GetPropertyValue<string>(propertyAlias);

                // get the raw value (this will be json)
                var urlValue = mediaItem.GetPropertyValue<string>(propertyAlias);

                mediaItemUrl = urlValue.DetectIsJson()
                    ? urlValue.SerializeToCropDataSet().Src
                    : urlValue;
            }
            else
            {
                mediaItemUrl = mediaItem.Url;
            }

            var cacheBusterValue = cacheBuster ? mediaItem.UpdateDate.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture) : null;

            return mediaItemUrl != null
                ? GetCropUrl(mediaItemUrl, width, height, imageCropperValue, cropAlias, quality, imageCropMode, imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBusterValue, furtherOptions, ratioMode, upScale)
                : string.Empty;
        }

        /// <summary>
        /// Gets the ImageProcessor Url from the image path.
        /// </summary>
        /// <param name="imageUrl">
        /// The image url.
        /// </param>
        /// <param name="width">
        /// The width of the output image.
        /// </param>
        /// <param name="height">
        /// The height of the output image.
        /// </param>
        /// <param name="imageCropperValue">
        /// The Json data from the Umbraco Core Image Cropper property editor
        /// </param>
        /// <param name="cropAlias">
        /// The crop alias.
        /// </param>
        /// <param name="quality">
        /// Quality percentage of the output image.
        /// </param>
        /// <param name="imageCropMode">
        /// The image crop mode.
        /// </param>
        /// <param name="imageCropAnchor">
        /// The image crop anchor.
        /// </param>
        /// <param name="preferFocalPoint">
        /// Use focal point to generate an output image using the focal point instead of the predefined crop if there is one
        /// </param>
        /// <param name="useCropDimensions">
        /// Use crop dimensions to have the output image sized according to the predefined crop sizes, this will override the width and height parameters
        /// </param>
        /// <param name="cacheBusterValue">
        /// Add a serialised date of the last edit of the item to ensure client cache refresh when updated
        /// </param>
        /// <param name="furtherOptions">
        /// The further options.
        /// </param>
        /// <param name="ratioMode">
        /// Use a dimension as a ratio
        /// </param>
        /// <param name="upScale">
        /// If the image should be upscaled to requested dimensions
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCropUrl(
            this string imageUrl,
            int? width = null,
            int? height = null,
            string imageCropperValue = null,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            string cacheBusterValue = null, 
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true)
        {
            if (string.IsNullOrEmpty(imageUrl) == false)
            {
                var imageProcessorUrl = new StringBuilder();

                if (string.IsNullOrEmpty(imageCropperValue) == false && imageCropperValue.DetectIsJson() && (imageCropMode == ImageCropMode.Crop || imageCropMode == null))
                {
                    var cropDataSet = imageCropperValue.SerializeToCropDataSet();
                    if (cropDataSet != null)
                    {
                        var crop = cropDataSet.GetCrop(cropAlias);

                        imageProcessorUrl.Append(cropDataSet.Src);

                        var cropBaseUrl = cropDataSet.GetCropBaseUrl(cropAlias, preferFocalPoint);
                        if (cropBaseUrl != null)
                        {
                            imageProcessorUrl.Append(cropBaseUrl);
                        }
                        else
                        {
                            return null;
                        }

                        if (crop != null & useCropDimensions)
                        {
                            width = crop.Width;
                            height = crop.Height;
                        }

                        // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a width parameter has been passed we can get the crop ratio for the height
                        if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width != null && height == null)
                        {
                            var heightRatio = (decimal)crop.Height / (decimal)crop.Width;
                            imageProcessorUrl.Append("&heightratio=" + heightRatio.ToString(CultureInfo.InvariantCulture));
                        }

                        // If a predefined crop has been specified & there are no coordinates & no ratio mode, but a height parameter has been passed we can get the crop ratio for the width
                        if (crop != null && string.IsNullOrEmpty(cropAlias) == false && crop.Coordinates == null && ratioMode == null && width == null && height != null)
                        {
                            var widthRatio = (decimal)crop.Width / (decimal)crop.Height;
                            imageProcessorUrl.Append("&widthratio=" + widthRatio.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
                else
                {
                    imageProcessorUrl.Append(imageUrl);

                    if (imageCropMode == null)
                    {
                        imageCropMode = ImageCropMode.Pad;
                    }

                    imageProcessorUrl.Append("?mode=" + imageCropMode.ToString().ToLower());

                    if (imageCropAnchor != null)
                    {
                        imageProcessorUrl.Append("&anchor=" + imageCropAnchor.ToString().ToLower());
                    }
                }

                if (quality != null)
                {
                    imageProcessorUrl.Append("&quality=" + quality);
                }

                if (width != null && ratioMode != ImageCropRatioMode.Width)
                {
                    imageProcessorUrl.Append("&width=" + width);
                }

                if (height != null && ratioMode != ImageCropRatioMode.Height)
                {
                    imageProcessorUrl.Append("&height=" + height);
                }

                if (ratioMode == ImageCropRatioMode.Width && height != null)
                {
                    // if only height specified then assume a sqaure
                    if (width == null)
                    {
                        width = height;
                    }

                    var widthRatio = (decimal)width / (decimal)height;
                    imageProcessorUrl.Append("&widthratio=" + widthRatio.ToString(CultureInfo.InvariantCulture));
                }

                if (ratioMode == ImageCropRatioMode.Height && width != null)
                {
                    // if only width specified then assume a sqaure
                    if (height == null)
                    {
                        height = width;
                    }

                    var heightRatio = (decimal)height / (decimal)width;
                    imageProcessorUrl.Append("&heightratio=" + heightRatio.ToString(CultureInfo.InvariantCulture));
                }

                if (upScale == false)
                {
                    imageProcessorUrl.Append("&upscale=false");                    
                }

                if (furtherOptions != null)
                {
                    imageProcessorUrl.Append(furtherOptions);
                }

                if (cacheBusterValue != null)
                {
                    imageProcessorUrl.Append("&rnd=").Append(cacheBusterValue);
                }

                return imageProcessorUrl.ToString();
            }

            return string.Empty;
        }
    }
}
