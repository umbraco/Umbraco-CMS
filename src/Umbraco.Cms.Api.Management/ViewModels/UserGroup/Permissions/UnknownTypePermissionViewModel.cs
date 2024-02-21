namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class UnknownTypePermissionViewModel : IPermissionViewModel
{
    public required ISet<string> Verbs { get; set; }

    public required string Context { get; set; }
}
