using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaEditingFactory
{
    MediaCreateModel MapCreateModel(MediaCreateRequestModel createRequestModel);

    MediaUpdateModel MapUpdateModel(MediaUpdateRequestModel updateRequestModel);
}
