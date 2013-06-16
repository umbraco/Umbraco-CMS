using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "user", Namespace = "")]
    public class UserDetail : UserBasic
    {
        [DataMember(Name = "email", IsRequired = true)]
        [Required]
        public string Email { get; set; }

        [DataMember(Name = "locale", IsRequired = true)]
        [Required]
        public string Language { get; set; }
    }
}