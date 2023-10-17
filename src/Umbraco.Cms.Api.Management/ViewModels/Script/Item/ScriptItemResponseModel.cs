using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Script.Item;

public class ScriptItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.ResourceObjectTypes.Script;
}
