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

            if ((useFocalPoint && HasFocalPoint()) || (crop != null && crop.Coordinates == null && HasFocalPoint()) || (string.IsNullOrEmpty(alias) && HasFocalPoint()))
            {
                sb.Append("?center=" + FocalPoint.Top.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + FocalPoint.Left.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append("&mode=crop");
            }
            else if (crop != null && crop.Coordinates != null)
            {
                sb.Append("?crop=");
                sb.Append(crop.Coordinates.X1.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                sb.Append(crop.Coordinates.Y1.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                sb.Append(crop.Coordinates.X2.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                sb.Append(crop.Coordinates.Y2.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append("&cropmode=percentage");
            }
            else
            {                
                sb.Append("?anchor=center");
                sb.Append("&mode=crop");
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