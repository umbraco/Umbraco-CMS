namespace Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;

public class DocumentTypePermissionPresentationModel : IPermissionPresentationModel
{
    public required ReferenceByIdModel DocumentType { get; set; }

    public required ReferenceByIdModel PropertyType { get; set; }

    public required ISet<string> Verbs { get; set; }
}
