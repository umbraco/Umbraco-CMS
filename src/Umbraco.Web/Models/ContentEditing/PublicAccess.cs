using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "publicAccess", Namespace = "")]
    public class PublicAccess
    {
        //[DataMember(Name = "userName")]
        //public string UserName { get; set; }

        [DataMember(Name = "roles")]
        public string[] Roles { get; set; }

        [DataMember(Name = "loginPage")]
        public EntityBasic LoginPage { get; set; }

        [DataMember(Name = "errorPage")]
        public EntityBasic ErrorPage { get; set; }

        [DataMember(Name = "members")]
        public MemberDisplay[] Members { get; set; }
    }
}
