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


        public string GetCropUrl(string alias)
        {

            var crop = Crops.GetCrop(alias);
            if(crop == null)
                return null;


            StringBuilder sb = new StringBuilder();
            if (crop.Coordinates != null)
            {
                sb.Append("?crop=");
                sb.Append(crop.Coordinates.X1).Append(",");
                sb.Append(crop.Coordinates.Y1).Append(",");
                sb.Append(crop.Coordinates.X2).Append(",");
                sb.Append(crop.Coordinates.Y2);
                sb.Append("&cropmode=percentage");
            }
            else
            {
                if (HasFocalPoint())
                {
                    sb.Append("?center=" + FocalPoint.Top + "," + FocalPoint.Left);
                    sb.Append("&mode=crop");
                }
                else
                {
                    sb.Append("?anchor=center");
                    sb.Append("&mode=crop");
                }

            }

            sb.Append("&width=").Append(crop.Width);
            sb.Append("&height=").Append(crop.Height);
            sb.Append("&rnd=").Append(DateTime.Now.Ticks);
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