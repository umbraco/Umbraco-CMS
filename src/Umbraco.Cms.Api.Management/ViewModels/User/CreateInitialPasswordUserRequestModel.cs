namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CreateInitialPasswordUserRequestModel : VerifyInviteUserRequestModel
{
    public string Password { get; set; } = string.Empty;
}
