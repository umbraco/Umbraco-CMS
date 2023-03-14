namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class EntityTreeItemResponseModel : TreeItemPresentationModel, INamedEntityPresentationModel
{
    public Guid Key { get; set; }

    public bool IsContainer { get; set; }

    public Guid? ParentKey { get; set; }
}
