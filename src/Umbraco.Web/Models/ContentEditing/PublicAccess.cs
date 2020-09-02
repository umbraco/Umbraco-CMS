using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "publicAccess", Namespace = "")]
    public class PublicAccess
    {

        [DataMember(Name = "roles")]
        public RoleDisplay[] Roles { get; set; }

        [DataMember(Name = "allRoles")]
        public RoleDisplay[] AllRoles { get; set; }

        [DataMember(Name = "loginPage")]
        public EntityBasic LoginPage { get; set; }

        [DataMember(Name = "errorPage")]
        public EntityBasic ErrorPage { get; set; }

        [DataMember(Name = "members")]
        public MemberDisplay[] Members { get; set; }


    }
}
