using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string? Icon { get; set; }
    public override string Type => Constants.UdiEntityType.DataType;
}
