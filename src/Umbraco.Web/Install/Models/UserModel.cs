using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "user", Namespace = "")]
    public class UserModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        
        [DataMember(Name = "email")]
        public string Email { get; set; }
        
        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "subscribeToNewsLetter")]
        public bool SubscribeToNewsLetter { get; set; }
    }
}