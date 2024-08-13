namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class ResendInviteUserRequestModel
{
    public required ReferenceByIdModel User { get; set; }

    public string? Message { get; set; }
}
