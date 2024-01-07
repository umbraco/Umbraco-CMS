namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentBlueprintTreeItemResponseModel : NamedEntityTreeItemResponseModel
{
    public Guid DocumentTypeId { get; set; }

    public string DocumentTypeAlias { get; set; } = string.Empty;

    public string? DocumentTypeName { get; set; } = string.Empty;
}
