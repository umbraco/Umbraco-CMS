namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class EntityTreeItemResponseModel : TreeItemPresentationModel
{
    public Guid Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }
}
