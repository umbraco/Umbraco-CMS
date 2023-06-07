using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

public class UnLinkLoginModel
{
    [Required]
    [DataMember(Name = "loginProvider", IsRequired = true)]
    public required string LoginProvider { get; set; }

    [Required]
    [DataMember(Name = "providerKey", IsRequired = true)]
    public required string ProviderKey { get; set; }
}
