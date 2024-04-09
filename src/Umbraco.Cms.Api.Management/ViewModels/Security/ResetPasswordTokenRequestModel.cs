using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class ResetPasswordTokenRequestModel : VerifyResetPasswordTokenRequestModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
