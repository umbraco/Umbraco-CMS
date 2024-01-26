using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaTypeEditingPresentationFactory
{
    MediaTypeCreateModel MapCreateModel(CreateMediaTypeRequestModel requestModel);

    MediaTypeUpdateModel MapUpdateModel(UpdateMediaTypeRequestModel requestModel);

    IEnumerable<AvailableMediaTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults);
}
