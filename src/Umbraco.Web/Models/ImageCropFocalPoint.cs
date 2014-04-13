using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropFocalPoint")]
    public class ImageCropFocalPoint{

        [DataMember(Name = "left")]
        public decimal Left { get; set; }

        [DataMember(Name = "top")]
        public decimal Top { get; set; }
    }
}