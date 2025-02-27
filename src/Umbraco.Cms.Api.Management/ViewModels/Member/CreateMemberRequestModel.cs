using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class CreateMemberRequestModel : CreateContentRequestModelBase<MemberValueModel, MemberVariantRequestModel>
{
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public required ReferenceByIdModel MemberType { get; set; }

    public IEnumerable<Guid>? Groups { get; set; }

    public bool IsApproved { get; set; }
}
