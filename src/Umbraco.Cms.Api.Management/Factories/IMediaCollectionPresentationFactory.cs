using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaCollectionPresentationFactory : IContentCollectionPresentationFactory<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>
{
}
