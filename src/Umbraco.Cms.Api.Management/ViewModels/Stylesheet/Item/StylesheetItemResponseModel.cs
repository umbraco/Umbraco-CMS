using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;

public class StylesheetItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.Stylesheet;
}
