namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class DocumentPermissionViewModel : IPermissionViewModel
{
    public required ReferenceByIdModel Document { get; set; }
    public required ISet<string> Verbs { get; set; }
}
