using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DictionaryTreeItemResponseModel : EntityTreeItemResponseModel
{
    public override string Type => Constants.UdiEntityType.DictionaryItem;
}
