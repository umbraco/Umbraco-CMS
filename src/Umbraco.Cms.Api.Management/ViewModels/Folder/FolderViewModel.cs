namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderReponseModel : FolderModelBase, INamedEntityPresentationModel
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
}
