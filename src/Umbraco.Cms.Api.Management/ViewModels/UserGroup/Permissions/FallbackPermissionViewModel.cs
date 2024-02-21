namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class FallbackPermissionViewModel : IPermissionViewModel
{
    public required ISet<string> Verbs { get; set; }
}
