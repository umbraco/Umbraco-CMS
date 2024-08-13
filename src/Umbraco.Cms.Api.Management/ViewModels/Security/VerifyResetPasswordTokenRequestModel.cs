namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class VerifyResetPasswordTokenRequestModel
{
    public required ReferenceByIdModel User { get; set; }

    public required string ResetCode { get; set; }
}
