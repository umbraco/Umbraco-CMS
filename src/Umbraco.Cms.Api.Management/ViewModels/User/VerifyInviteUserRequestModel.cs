namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class VerifyInviteUserRequestModel
{
    public Guid UserId { get; set; } = Guid.Empty;

    public string Token { get; set; } = string.Empty;
}
