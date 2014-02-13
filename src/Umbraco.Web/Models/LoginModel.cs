using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.Models
{
    public class LoginModel : PostRedirectModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
