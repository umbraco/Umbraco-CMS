using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Umbraco.Web.UmbracoModels
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
