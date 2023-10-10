using Umbraco.Cms.Api.Management.ViewModels.TextFiles;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Script;

public class ScriptResponseModel : TextFileResponseModelBase
{
    public override string Type => Constants.UdiEntityType.Script;
}
