namespace Umbraco.Cms.ManagementApi.ViewModels.Tree;

public class DocumentBlueprintTreeItemViewModel : EntityTreeItemViewModel
{
    public Guid DocumentTypeKey { get; set; }

    public string DocumentTypeAlias { get; set; } = string.Empty;

    public string? DocumentTypeName { get; set; } = string.Empty;
}
