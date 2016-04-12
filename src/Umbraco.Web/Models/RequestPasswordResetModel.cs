using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    public class RequestPasswordResetModel
    {
        [Required]
        [DataMember(Name = "email", IsRequired = true)]
        public string Email { get; set; }
    }
}
