using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    public class LoginModel : PostRedirectModel
    {
        [Required]
        [DataMember(Name = "username", IsRequired = true)]
        public string Username { get; set; }

        [Required]
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }

    }
}
