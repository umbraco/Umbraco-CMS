using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentVersionPresentationFactory
{
    Task<DocumentVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion);

    Task<IEnumerable<DocumentVersionItemResponseModel>> CreateMultipleAsync(
        IEnumerable<ContentVersionMeta> contentVersions);
}
