using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Basic member property type
    /// </summary>
    [DataContract(Name = "contentType", Namespace = "")]
    public class MemberPropertyTypeBasic : PropertyTypeBasic
    {
        [DataMember(Name = "showOnMemberProfile")]
        public bool MemberCanViewProperty { get; set; }

        [DataMember(Name = "memberCanEdit")]
        public bool MemberCanEditProperty { get; set; }
    }
}