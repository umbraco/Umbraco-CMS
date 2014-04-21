using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace Umbraco.Web.Models
{
    [DataContract(Name="imageCropDataSet")]
    public class ImageCropDataSet : IHtmlString
    {
        [DataMember(Name="src")]
        public string Src { get; set;}

        [DataMember(Name = "focalPoint")]
        public ImageCropFocalPoint FocalPoint { get; set; }

        [DataMember(Name = "crops")]
        public IEnumerable<ImageCropData> Crops { get; set; }


        public string GetCropUrl(string alias, bool useCropDimensions = true, bool useFocalPoint = false, string cacheBusterValue = null)
        {

            var crop = Crops.GetCrop(alias);

            if (crop == null && string.IsNullOrEmpty(alias) == false)
            {
                return null;
            }

            var sb = new StringBuilder();

            var cropBaseUrl = this.GetCropBaseUrl(alias, useFocalPoint);
            if (cropBaseUrl != null)
            {
                sb.Append(cropBaseUrl);
            }

            if (crop != null && useCropDimensions)
            {
                sb.Append("&width=").Append(crop.Width);
                sb.Append("&height=").Append(crop.Height);
            }

            if (cacheBusterValue != null)
            {
                sb.Append("&rnd=").Append(cacheBusterValue);
            }

            return sb.ToString();

        }

        public bool HasFocalPoint()
        {
            return FocalPoint != null && FocalPoint.Top != 0.5m && FocalPoint.Top != 0.5m;
        }

        public bool HasCrop(string alias)
        {
            return Crops.Any(x => x.Alias == alias);
        }

        public bool HasImage()
        {
            return string.IsNullOrEmpty(Src);
        }

        public string ToHtmlString()
        {
            return this.Src;
        }
    }
}