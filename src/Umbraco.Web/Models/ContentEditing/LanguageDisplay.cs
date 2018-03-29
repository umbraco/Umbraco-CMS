using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "language", Namespace = "")]
    public class LanguageDisplay
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "culture")]
        public string IsoCode { get; set; }

        [DataMember(Name = "cultureDisplayName")]
        public string Name { get; set; }

        [DataMember(Name = "isDefault")]
        public bool IsDefaultVariantLanguage { get; set; }

        [DataMember(Name = "isMandatory")]
        public bool Mandatory { get; set; }
    }
}
