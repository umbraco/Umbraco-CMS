using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class MemberGroupTreeItemResponseModel : EntityTreeItemResponseModel
{
    public override string Type => Constants.UdiEntityType.MemberGroup;
}
