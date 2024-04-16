using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class UnLinkLoginRequestModel
{
    [Required]
    public required string LoginProvider { get; set; }

    [Required]
    public required string ProviderKey { get; set; }
}
