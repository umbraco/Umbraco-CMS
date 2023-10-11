using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class RelationTypeTreeItemResponseModel : EntityTreeItemResponseModel
{
    public override string Type => Constants.UdiEntityType.RelationType;
}
