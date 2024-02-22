namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class FallbackPermissionPresentationModel : IPermissionPresentationModel
{
    public required ISet<string> Verbs { get; set; }
}
