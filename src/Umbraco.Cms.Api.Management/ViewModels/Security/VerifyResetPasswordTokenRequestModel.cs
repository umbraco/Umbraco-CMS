using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class VerifyResetPasswordTokenRequestModel
{
    public Guid UserId { get; set; }
    [Required]
    public string ResetCode { get; set; } = string.Empty;
}
