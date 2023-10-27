namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class DisableUserRequestModel
{
    public HashSet<Guid> UserIds { get; set; } = new();
}
