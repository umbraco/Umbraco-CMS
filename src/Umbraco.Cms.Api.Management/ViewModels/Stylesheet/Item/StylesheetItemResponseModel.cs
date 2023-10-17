using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;

public class StylesheetItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.ResourceObjectTypes.Stylesheet;
}
