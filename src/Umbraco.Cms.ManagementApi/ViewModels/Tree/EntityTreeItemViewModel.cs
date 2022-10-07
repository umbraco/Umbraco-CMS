namespace Umbraco.Cms.ManagementApi.ViewModels.Tree;

public class EntityTreeItemViewModel : TreeItemViewModel
{
    public Guid Key { get; set; }

    public bool IsContainer { get; set; }

    public Guid? ParentKey { get; set; }
}
