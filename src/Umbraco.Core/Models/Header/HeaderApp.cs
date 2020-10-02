using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Header
{
    [DataContract(Name = "headerApp", Namespace = "")]
    public class HeaderApp
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "weight")]
        public int Weight { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "hotkey")]
        public string Hotkey { get; set; }

        [DataMember(Name = "action")]
        public string Action { get; set; }
    }
}
