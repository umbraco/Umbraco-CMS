using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "publicAccess", Namespace = "")]
    public class PublicAccess
    {
        [DataMember(Name = "Id")]
        public int Id { get; set; }

        [DataMember(Name = "NodeName")]
        public string NodeName { get; set; }

        [DataMember(Name = "groups")]
        public MemberGroupDisplay[] Groups { get; set; }

        [DataMember(Name = "loginPage")]
        public EntityBasic LoginPage { get; set; }

        [DataMember(Name = "errorPage")]
        public EntityBasic ErrorPage { get; set; }

        [DataMember(Name = "members")]
        public MemberDisplay[] Members { get; set; }
    }
}
