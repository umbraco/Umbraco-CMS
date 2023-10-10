using Umbraco.Cms.Api.Management.ViewModels.TextFiles;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Stylesheet;

public class StylesheetResponseModel : TextFileResponseModelBase
{
    public override string Type => Constants.UdiEntityType.Stylesheet;
}
