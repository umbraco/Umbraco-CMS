namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class ResetPasswordTokenRequestModel : VerifyResetPasswordTokenRequestModel
{
    public string Password { get; set; } = string.Empty;
}
