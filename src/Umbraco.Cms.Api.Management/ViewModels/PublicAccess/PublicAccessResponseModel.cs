using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessResponseModel
{
    public MemberItemResponseModel[] Members { get; set; } = Array.Empty<MemberItemResponseModel>();

    public MemberGroupItemReponseModel[] Groups { get; set; } = Array.Empty<MemberGroupItemReponseModel>();

    public ContentTreeItemResponseModel? LoginPage { get; set; }

    public ContentTreeItemResponseModel? ErrorPage { get; set; }
}
