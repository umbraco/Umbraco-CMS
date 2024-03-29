using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

[ShortGenericSchemaName<MemberValueModel, MemberVariantRequestModel>("CreateContentForMemberRequestModel")]
public class CreateMemberRequestModel : CreateContentRequestModelBase<MemberValueModel, MemberVariantRequestModel>
{
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public required ReferenceByIdModel MemberType { get; set; }

    public IEnumerable<string>? Groups { get; set; }

    public bool IsApproved { get; set; }
}
