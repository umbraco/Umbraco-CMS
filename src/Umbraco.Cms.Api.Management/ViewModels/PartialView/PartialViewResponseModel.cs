using Umbraco.Cms.Api.Management.ViewModels.TextFiles;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView;

public class PartialViewResponseModel : TextFileResponseModelBase
{
    public override string Type => Constants.ResourceObjectTypes.PartialView;
}
