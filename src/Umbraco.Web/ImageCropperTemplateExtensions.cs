using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web
{
    public static class ImageCropperTemplateExtensions
    {
        
        //this only takes the crop json into account
        public static string Crop(this IPublishedContent mediaItem, string propertyAlias, string cropAlias)
        {
            mediaItem.HasProperty(propertyAlias);
            var property = mediaItem.GetPropertyValue<string>(propertyAlias);

            if (string.IsNullOrEmpty(property))
                return string.Empty;

            if (property.IsJson())
            {
                var cropDataSet = property.SerializeToCropDataSet();
                var currentCrop = cropDataSet.Crops.First(x => x.Alias ==cropAlias);
                return cropDataSet.Src + currentCrop.ToUrl();
            }
            else
            {
                //must be a string
                var cropData = ImageCropperPropertyEditorHelper.GetCrop(mediaItem.ContentType.Alias, cropAlias);
                return property + cropData.ToUrl();
            }
        }


       public static string Crop(
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

            return mediaItem != null ? Crop(mediaItem.Url, width, height, quality, mode, anchor, imageCropperValue, imageCropperCropId, furtherOptions, slimmage) : string.Empty;
        }


        public static string Crop(
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

                if (!string.IsNullOrEmpty(imageCropperValue) && imageCropperValue.IsJson())
                {
                    var allTheCrops = imageCropperValue.SerializeToCropDataSet();
                    if (allTheCrops != null && allTheCrops.Crops.Any())
                    {
                        var crop = cropAlias != null
                                       ? allTheCrops.Crops.First(x => x.Alias ==cropAlias)
                                       : allTheCrops.Crops.First();
                        if (crop != null)
                        {
                            imageResizerUrl.Append(crop.ToUrl());
                        }
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
