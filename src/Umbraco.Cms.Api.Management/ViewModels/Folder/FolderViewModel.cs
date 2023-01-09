namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderViewModel : FolderModelBase
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }
}
