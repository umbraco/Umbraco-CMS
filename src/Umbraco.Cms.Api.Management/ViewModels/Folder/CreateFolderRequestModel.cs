namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class CreateFolderRequestModel : FolderModelBase
{
    public Guid? Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }
}
