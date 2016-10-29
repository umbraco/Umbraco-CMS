using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyType")]
    public class MemberPropertyTypeDisplay : PropertyTypeDisplay
    {
        [DataMember(Name = "showOnMemberProfile")]
        public bool MemberCanViewProperty { get; set; }

        [DataMember(Name = "memberCanEdit")]
        public bool MemberCanEditProperty { get; set; }
    }
}