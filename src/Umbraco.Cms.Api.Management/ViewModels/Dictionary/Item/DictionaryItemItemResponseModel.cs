using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;

public class DictionaryItemItemResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.DictionaryItem;
}
