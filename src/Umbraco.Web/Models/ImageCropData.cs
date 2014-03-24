using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "imageCropData")]
    public class ImageCropData
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        //[DataMember(Name = "name")]
        //public string Name { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "coordinates")]
        public ImageCropCoordinates Coordinates { get; set; }
    }

}
