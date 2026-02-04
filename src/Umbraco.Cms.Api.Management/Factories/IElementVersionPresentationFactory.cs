using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IElementVersionPresentationFactory
{
    Task<ElementVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion);

    Task<IEnumerable<ElementVersionItemResponseModel>> CreateMultipleAsync(
        IEnumerable<ContentVersionMeta> contentVersions);
}
