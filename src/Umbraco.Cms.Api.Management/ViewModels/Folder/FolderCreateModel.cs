namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class CreateFolderRequestModel : FolderModelBase
{
    public Guid? ParentKey { get; set; }
}
