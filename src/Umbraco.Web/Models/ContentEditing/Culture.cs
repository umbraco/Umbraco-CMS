using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{

    [DataContract(Name = "culture", Namespace = "")]
    public class Culture
    {
        [DataMember(Name = "culture")]
        public string IsoCode { get; set; }

        [DataMember(Name = "cultureDisplayName")]
        public string Name { get; set; }
    }
}
