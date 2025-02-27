using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

public class UpdateMemberTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMemberTypePropertyTypeRequestModel, UpdateMemberTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
