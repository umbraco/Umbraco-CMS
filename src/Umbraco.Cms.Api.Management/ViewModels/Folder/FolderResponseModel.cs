namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class FolderResponseModel : FolderModelBase
{
    public Guid Id { get; set; }

    public bool IsTrashed { get; set; }
}
