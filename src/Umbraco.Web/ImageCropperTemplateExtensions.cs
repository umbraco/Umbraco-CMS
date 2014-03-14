using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web
{
    public static class ImageCropperTemplateExtensions
    {
        
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
            Mode? mode = null,
            Anchor? anchor = null,
            string imageCropperAlias = null,
            string imageCropperCropId = null,
            string furtherOptions = null,
            bool slimmage = false)
        {
            string imageCropperValue = null;

            if (mediaItem.HasPropertyAndValueAndCrop(imageCropperAlias, imageCropperCropId))
            {
                imageCropperValue = mediaItem.GetPropertyValueHack(imageCropperAlias);
            }

            return mediaItem != null ? GetCropUrl(mediaItem.Url, width, height, quality, mode, anchor, imageCropperValue, imageCropperCropId, furtherOptions, slimmage) : string.Empty;
        }


        public static string GetCropUrl(
            this string imageUrl,
            int? width = null,
            int? height = null,
            int? quality = null,
            Mode? mode = null,
            Anchor? anchor = null,
            string imageCropperValue = null,
            string cropAlias = null,
            string furtherOptions = null,
            bool slimmage = false)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageResizerUrl = new StringBuilder();
                imageResizerUrl.Append(imageUrl);

                if (!string.IsNullOrEmpty(imageCropperValue) && imageCropperValue.DetectIsJson())
                {
                    var allTheCrops = imageCropperValue.SerializeToCropDataSet();
                    if (allTheCrops != null && allTheCrops.Crops.Any())
                    {

                        if(allTheCrops.HasCrop(cropAlias))
                            imageResizerUrl.Append(allTheCrops.GetCropUrl(cropAlias));
                    }
                }
                else
                {
                    if (mode == null)
                    {
                        mode = Mode.Pad;
                    }
                    imageResizerUrl.Append("?mode=" + mode.ToString().ToLower());

                    if (anchor != null)
                    {
                        imageResizerUrl.Append("&anchor=" + anchor.ToString().ToLower());
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

                if (slimmage)
                {
                    if (width == null)
                    {
                        imageResizerUrl.Append("&width=300");
                    }
                    if (quality == null)
                    {
                        imageResizerUrl.Append("&quality=90");
                    }
                    imageResizerUrl.Append("&slimmage=true");
                }

                if (furtherOptions != null)
                {
                    imageResizerUrl.Append(furtherOptions);
                }

                return imageResizerUrl.ToString();

            }
            return string.Empty;
        }



        public enum Mode
        {
            Crop,
            Max,
            Strech,
            Pad
        }

        public enum Anchor
        {
            Center,
            Top,
            Right,
            Bottom,
            Left
        }
    }
}
