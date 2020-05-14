using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "publicAccess", Namespace = "")]
    public class PublicAccess
    {

        [DataMember(Name = "groups")]
        public RoleDisplay[] Groups { get; set; }

        [DataMember(Name = "allGroups")]
        public RoleDisplay[] AllGroups { get; set; }

        [DataMember(Name = "hasProtection")]
        public bool HasProtection { get; set; }

        [DataMember(Name = "loginPage")]
        public EntityBasic LoginPage { get; set; }

        [DataMember(Name = "errorPage")]
        public EntityBasic ErrorPage { get; set; }

        [DataMember(Name = "members")]
        public MemberDisplay[] Members { get; set; }


    }
}
