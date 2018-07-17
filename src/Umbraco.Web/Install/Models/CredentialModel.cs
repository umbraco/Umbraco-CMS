using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "credential", Namespace = "")]
    public class CredentialModel
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }
        
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}
