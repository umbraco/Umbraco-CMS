using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    [DataContract(Name = "headerApp", Namespace = "")]
    public class HeaderApp
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "weight")]
        public int Weight { get; set; }

        [DataMember(Name = "view")]
        public string View { get; set; }
    }
}
