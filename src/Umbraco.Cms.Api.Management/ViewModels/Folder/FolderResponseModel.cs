namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderResponseModel : FolderModelBase, INamedEntityPresentationModel
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
}
