using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

public class MemberTypeResponseModel : ContentTypeResponseModelBase<MemberTypePropertyTypeResponseModel, MemberTypePropertyTypeContainerResponseModel>
{
    public IEnumerable<MemberTypeComposition> Compositions { get; set; } = Enumerable.Empty<MemberTypeComposition>();
}
