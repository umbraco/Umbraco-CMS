namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderReponseModel : FolderModelBase, INamedEntityPresentationModel
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }
}
