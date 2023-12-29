using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class ResetPasswordRequestModel
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
