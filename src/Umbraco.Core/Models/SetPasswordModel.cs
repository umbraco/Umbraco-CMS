using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract(Name = "setPassword", Namespace = "")]
public class SetPasswordModel
{
    [Required]
    [DataMember(Name = "userId", IsRequired = true)]
    public int UserId { get; set; }

    [Required]
    [DataMember(Name = "password", IsRequired = true)]
    public string? Password { get; set; }

    [Required]
    [DataMember(Name = "resetCode", IsRequired = true)]
    public string? ResetCode { get; set; }
}
