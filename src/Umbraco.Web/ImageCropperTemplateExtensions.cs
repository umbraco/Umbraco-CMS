using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web
{
    public static class ImageCropperTemplateExtensions
    {

        public static string GetCropUrl(this IPublishedContent mediaItem, string cropAlias)
        {
            return mediaItem.GetCropUrl(Constants.Conventions.Media.File, cropAlias);
        }

        // this only takes the crop json into account
        public static string GetCropUrl(this IPublishedContent mediaItem, string propertyAlias, string cropAlias)
        {
            return mediaItem.GetCropUrl(propertyAlias: propertyAlias, cropAlias: cropAlias, useCropDimensions: true);
        }

        public static string GetCropUrl(
             this IPublishedContent mediaItem,
             int? width = null,
             int? height = null,
             int? quality = null,
             ImageCropMode? imageCropMode = null,
             ImageCropAnchor? imageCropAnchor = null,
             string propertyAlias = Constants.Conventions.Media.File,
             string cropAlias = null,
             bool useFocalPoint = false,
             bool useCropDimensions = false,
             bool cacheBuster = true, 
             string furtherOptions = null)
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

            return mediaItemUrl != null
                ? GetCropUrl(mediaItemUrl, width, height, quality, imageCropMode, imageCropAnchor, imageCropperValue, cropAlias, useFocalPoint,  useCropDimensions, cacheBuster, furtherOptions)
                : string.Empty;
        }

        public static string GetCropUrl(
            this string imageUrl,
            int? width = null,
            int? height = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            string imageCropperValue = null,
            string cropAlias = null,
            bool useFocalPoint = false,
            bool useCropDimensions = false,
            bool cacheBuster = true, 
            string furtherOptions = null)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageResizerUrl = new StringBuilder();

                if (!string.IsNullOrEmpty(imageCropperValue) && imageCropperValue.DetectIsJson())
                {
                    var cropDataSet = imageCropperValue.SerializeToCropDataSet();
                    if (cropDataSet != null)
                    {
                        var cropUrl = cropDataSet.GetCropUrl(cropAlias, false, useFocalPoint, cacheBuster);
                        
                        // if crop alias has been specified but not found we should return null
                        if (string.IsNullOrEmpty(cropAlias) == false && cropUrl == null)
                        {
                            return null;
                        }
                        imageResizerUrl.Append(cropDataSet.Src);
                        imageResizerUrl.Append(cropDataSet.GetCropUrl(cropAlias, useCropDimensions, useFocalPoint, cacheBuster));
                    }
                }
                else
                {
                    imageResizerUrl.Append(imageUrl);
                    if (imageCropMode == null)
                    {
                        imageCropMode = ImageCropMode.Pad;
                    }
                    imageResizerUrl.Append("?mode=" + imageCropMode.ToString().ToLower());

                    if (imageCropAnchor != null)
                    {
                        imageResizerUrl.Append("&anchor=" + imageCropAnchor.ToString().ToLower());
                    }
                }

                if (quality != null)
                {
                    imageResizerUrl.Append("&quality=" + quality);
                }

                if (width != null && useCropDimensions == false)
                {
                    imageResizerUrl.Append("&width=" + width);
                }

                if (height != null && useCropDimensions == false)
                {
                    imageResizerUrl.Append("&height=" + height);
                }

                if (furtherOptions != null)
                {
                    imageResizerUrl.Append(furtherOptions);
                }

                return imageResizerUrl.ToString();

            }
            return string.Empty;
        }

    }
}
