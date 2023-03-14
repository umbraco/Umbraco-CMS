using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(Name = "validatePasswordReset", Namespace = "")]
public class ValidatePasswordResetCodeModel
{
    [Required]
    [DataMember(Name = "userId", IsRequired = true)]
    public int UserId { get; set; }

    [Required]
    [DataMember(Name = "resetCode", IsRequired = true)]
    public string? ResetCode { get; set; }
}
