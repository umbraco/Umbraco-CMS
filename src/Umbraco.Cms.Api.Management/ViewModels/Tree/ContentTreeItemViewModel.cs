namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class ContentTreeItemViewModel : EntityTreeItemViewModel
{
    public bool NoAccess { get; set; }

    public bool IsTrashed { get; set; }
}
