using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.RelationType.Item;

public class RelationTypeItemResponseModel : ItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.RelationType;
}
