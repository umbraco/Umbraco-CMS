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

        //this only takes the crop json into account
        public static string GetCropUrl(this IPublishedContent mediaItem, string propertyAlias, string cropAlias)
        {
            mediaItem.HasProperty(propertyAlias);
            var property = mediaItem.GetPropertyValue<string>(propertyAlias);

            if (string.IsNullOrEmpty(property))
                return string.Empty;

            if (property.DetectIsJson())
            {
                var cropDataSet = property.SerializeToCropDataSet();
                return cropDataSet.Src + cropDataSet.GetCropUrl(cropAlias);
            }
            else
            {
                return property;
            }
        }

        public static string GetCropUrl(
             this IPublishedContent mediaItem,
             int? width = null,
             int? height = null,
             int? quality = null,
             ImageCropMode? imageCropMode = null,
             ImageCropAnchor? imageCropAnchor = null,
             string propertyAlias = null,
             string cropAlias = null,
             string furtherOptions = null)
        {
            string imageCropperValue = null;

            string mediaItemUrl;

            if (mediaItem.HasPropertyAndValueAndCrop(propertyAlias, cropAlias))
            {
                imageCropperValue = mediaItem.GetPropertyValue<string>(propertyAlias);

                //get the raw value (this will be json)
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
                ? GetCropUrl(mediaItemUrl, width, height, quality, imageCropMode, imageCropAnchor, imageCropperValue, cropAlias, furtherOptions)
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
            string furtherOptions = null)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageResizerUrl = new StringBuilder();

                if (!string.IsNullOrEmpty(imageCropperValue) && imageCropperValue.DetectIsJson())
                {
                    var cropDataSet = imageCropperValue.SerializeToCropDataSet();
                    imageResizerUrl.Append(cropDataSet.Src);
                    var crop = cropDataSet.Crops.FirstOrDefault(x => cropAlias != null && x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant());
                    if (crop != null && crop.Coordinates != null)
                    {
                        imageResizerUrl.Append("?crop=");
                        imageResizerUrl.Append(crop.Coordinates.X1).Append(",");
                        imageResizerUrl.Append(crop.Coordinates.Y1).Append(",");
                        imageResizerUrl.Append(crop.Coordinates.X2).Append(",");
                        imageResizerUrl.Append(crop.Coordinates.Y2);
                        imageResizerUrl.Append("&cropmode=percentage");
                    }
                    else
                    {
                        if (cropDataSet.HasFocalPoint())
                        {
                            imageResizerUrl.Append("?center=" + cropDataSet.FocalPoint.Top + "," + cropDataSet.FocalPoint.Left);
                            imageResizerUrl.Append("&mode=crop");
                        }
                        else
                        {
                            imageResizerUrl.Append("?anchor=center");
                            imageResizerUrl.Append("&mode=crop");
                        }

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

                if (width != null)
                {
                    imageResizerUrl.Append("&width=" + width);
                }

                if (height != null)
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
