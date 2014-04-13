using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropCoordinates")]
    public class ImageCropCoordinates
    {
        [DataMember(Name = "x1")]
        public decimal X1 { get; set; }

        [DataMember(Name = "y1")]
        public decimal Y1 { get; set; }

        [DataMember(Name = "x2")]
        public decimal X2 { get; set; }

        [DataMember(Name = "y2")]
        public decimal Y2 { get; set; }
    }
}