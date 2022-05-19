using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

public class UnLinkLoginModel
{
    [Required]
    [DataMember(Name = "loginProvider", IsRequired = true)]
    public string? LoginProvider { get; set; }

    [Required]
    [DataMember(Name = "providerKey", IsRequired = true)]
    public string? ProviderKey { get; set; }
}
