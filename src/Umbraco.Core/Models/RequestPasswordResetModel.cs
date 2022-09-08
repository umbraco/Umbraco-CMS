using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract(Name = "requestPasswordReset", Namespace = "")]
public class RequestPasswordResetModel
{
    [Required]
    [DataMember(Name = "email", IsRequired = true)]
    public string Email { get; set; } = null!;
}
