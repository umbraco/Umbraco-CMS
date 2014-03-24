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


        public string GetCropUrl(string alias, bool addCropDimensions = true, bool addRandom = true)
        {

            var crop = Crops.GetCrop(alias);
            if(crop == null)
                return null;


            var sb = new StringBuilder();
            if (crop.Coordinates != null)
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
                if (HasFocalPoint())
                {
                    sb.Append("?center=" + FocalPoint.Top.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + FocalPoint.Left.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    sb.Append("&mode=crop");
                }
                else
                {
                    sb.Append("?anchor=center");
                    sb.Append("&mode=crop");
                }

            }

            if (addCropDimensions)
            {
                sb.Append("&width=").Append(crop.Width);
                sb.Append("&height=").Append(crop.Height);
            }
            if (addRandom)
            {
                sb.Append("&rnd=").Append(DateTime.Now.Ticks);
            }
            return sb.ToString();

        }

        public bool HasFocalPoint()
        {
            return (FocalPoint != null && FocalPoint.Top != 0.5m && FocalPoint.Top != 0.5m);
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