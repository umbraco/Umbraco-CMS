using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class MemberTypeTreeItemResponseModel : EntityTreeItemResponseModel
{
    public override string Type => Constants.UdiEntityType.MemberType;
}
