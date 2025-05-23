using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaEditingPresentationFactory
{
    MediaCreateModel MapCreateModel(CreateMediaRequestModel createRequestModel);

    MediaUpdateModel MapUpdateModel(UpdateMediaRequestModel updateRequestModel);
}
