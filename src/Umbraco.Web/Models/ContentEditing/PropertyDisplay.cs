using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "porperty", Namespace = "")]
    public class PropertyDisplay : UserBasic
    {
        [DataMember(Name = "name")]
        public string Name{ get; set; }

        [DataMember(Name = "value")]
        public string Value{ get; set; }

    }
}
