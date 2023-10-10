using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Script.Item;

public class ScriptItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.Script;
}
