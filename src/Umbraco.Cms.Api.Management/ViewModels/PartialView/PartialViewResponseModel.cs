using Umbraco.Cms.Api.Management.ViewModels.TextFiles;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView;

public class PartialViewResponseModel : TextFileResponseModelBase
{
    public override string Type => Constants.UdiEntityType.PartialView;
}
