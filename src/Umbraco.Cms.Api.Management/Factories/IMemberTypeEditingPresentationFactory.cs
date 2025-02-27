using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMemberTypeEditingPresentationFactory
{
    MemberTypeCreateModel MapCreateModel(CreateMemberTypeRequestModel requestModel);

    MemberTypeUpdateModel MapUpdateModel(UpdateMemberTypeRequestModel requestModel);

    IEnumerable<AvailableMemberTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
