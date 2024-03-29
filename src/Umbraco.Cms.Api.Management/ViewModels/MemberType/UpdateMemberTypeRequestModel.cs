using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

[ShortGenericSchemaName<UpdateMemberTypePropertyTypeRequestModel, UpdateMemberTypePropertyTypeContainerRequestModel>("UpdateContentTypeForMemberTypeRequestModel")]
public class UpdateMemberTypeRequestModel
    : UpdateContentTypeRequestModelBase<UpdateMemberTypePropertyTypeRequestModel, UpdateMemberTypePropertyTypeContainerRequestModel>
{
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
