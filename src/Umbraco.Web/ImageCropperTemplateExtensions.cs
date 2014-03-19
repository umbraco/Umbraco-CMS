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
            string propertyAlias = null,
            string cropAlias = null,
            string furtherOptions = null)
        {
            string imageCropperValue = null;

           string mediaItemUrl = null;

            if (mediaItem.HasPropertyAndValueAndCrop(propertyAlias, cropAlias))
            {
                imageCropperValue = mediaItem.GetPropertyValue<string>(propertyAlias);
            }

           //this probably shouldn't be needed but it is currently as mediaItem.Url is populated with full crop JSON
           mediaItemUrl = mediaItem.Url.DetectIsJson() ? mediaItem.Url.SerializeToCropDataSet().Src : mediaItem.Url;

           return mediaItem != null ? GetCropUrl(mediaItemUrl, width, height, quality, mode, anchor, imageCropperValue, cropAlias, furtherOptions) : string.Empty;
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
            string furtherOptions = null)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageResizerUrl = new StringBuilder();
               // imageResizerUrl.Append(imageUrl);

                if (!string.IsNullOrEmpty(imageCropperValue) && imageCropperValue.DetectIsJson())
                {
                    var cropDataSet = imageCropperValue.SerializeToCropDataSet();
                    imageResizerUrl.Append(cropDataSet.Src);
                    var crop = cropDataSet.Crops.FirstOrDefault(x => x.Alias == cropAlias);
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
