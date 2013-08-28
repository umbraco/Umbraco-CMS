using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Umbraco.Web.UmbracoModels
{
    public class RegisterModel
    {
        [Required]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", 
            ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string MemberTypeAlias { get; set; }

        public List<UmbracoProperty> MemberProperties { get; set; }
    }

    public class UmbracoProperty
    {
        public string Alias { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
    }
}
