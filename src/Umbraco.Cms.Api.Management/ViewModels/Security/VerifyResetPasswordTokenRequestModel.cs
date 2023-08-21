namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class VerifyResetPasswordTokenRequestModel
{
    public Guid UserKey { get; set; }
    public string ResetCode { get; set; } = string.Empty;
}
