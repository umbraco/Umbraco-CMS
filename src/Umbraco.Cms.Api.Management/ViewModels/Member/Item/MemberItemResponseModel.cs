using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.Member.Item;

public class MemberItemResponseModel : ItemResponseModelBase
{
    public MemberTypeReferenceResponseModel MemberType { get; set; } = new();

    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();

    public MemberKind Kind { get; set; }
}
