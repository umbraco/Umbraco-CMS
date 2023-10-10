using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;

public class DocumentBlueprintResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.DocumentBlueprint;
}
