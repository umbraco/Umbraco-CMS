using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

public class CreateMemberTypeRequestModel
    : CreateContentTypeRequestModelBase<CreateMemberTypePropertyTypeRequestModel, CreateMemberTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
