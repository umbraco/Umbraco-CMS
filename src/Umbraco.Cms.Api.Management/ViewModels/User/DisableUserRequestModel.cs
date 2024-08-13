namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class DisableUserRequestModel
{
    public HashSet<ReferenceByIdModel> UserIds { get; set; } = new();
}
