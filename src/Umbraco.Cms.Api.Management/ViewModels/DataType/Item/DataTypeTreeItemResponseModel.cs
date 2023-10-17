using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string? Icon { get; set; }

    public override string Type => Constants.ResourceObjectTypes.DataType;
}
