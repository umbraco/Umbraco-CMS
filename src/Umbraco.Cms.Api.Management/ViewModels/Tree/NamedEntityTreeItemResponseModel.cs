namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class NamedEntityTreeItemResponseModel : EntityTreeItemResponseModel, INamedEntityPresentationModel
{
    public string Name { get; set; } = string.Empty;
}
