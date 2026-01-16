namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class ElementPermissionPresentationModel : IPermissionPresentationModel
{
    public required ReferenceByIdModel Element { get; set; }

    public required ISet<string> Verbs { get; set; }
}