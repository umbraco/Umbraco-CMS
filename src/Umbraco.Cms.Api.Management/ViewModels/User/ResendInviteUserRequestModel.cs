namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class ResendInviteUserRequestModel
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string? Message { get; set; }
}
