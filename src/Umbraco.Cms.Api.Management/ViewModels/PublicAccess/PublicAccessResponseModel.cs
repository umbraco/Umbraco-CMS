using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.PublicAccess;

public class PublicAccessResponseModel
{
    public MemberItemResponseModel[] Members { get; set; } = Array.Empty<MemberItemResponseModel>();

    public MemberGroupItemResponseModel[] Groups { get; set; } = Array.Empty<MemberGroupItemResponseModel>();

    public Guid LoginPageId { get; set; }

    public Guid ErrorPageId { get; set; }
}
