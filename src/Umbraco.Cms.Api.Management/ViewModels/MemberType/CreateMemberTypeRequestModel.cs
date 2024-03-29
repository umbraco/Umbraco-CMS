using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

[ShortGenericSchemaName<CreateMemberTypePropertyTypeRequestModel, CreateMemberTypePropertyTypeContainerRequestModel>("CreateContentTypeForMemberTypeRequestModel")]
public class CreateMemberTypeRequestModel
    : CreateContentTypeRequestModelBase<CreateMemberTypePropertyTypeRequestModel, CreateMemberTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
