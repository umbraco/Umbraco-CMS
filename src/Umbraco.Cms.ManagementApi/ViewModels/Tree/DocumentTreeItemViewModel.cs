namespace Umbraco.Cms.ManagementApi.ViewModels.Tree;

public class DocumentTreeItemViewModel : ContentTreeItemViewModel
{
    public bool IsProtected { get; set; }

    public bool IsPublished { get; set; }

    public bool IsEdited { get; set; }
}
