using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentVersionPresentationFactory
{
    Task<DocumentVersionItemResponseModel> CreateResponseModelAsync(ContentVersionMeta contentVersion);

    Task<PagedViewModel<DocumentVersionItemResponseModel>> CreatedPagedResponseModelAsync(
        PagedModel<ContentVersionMeta> pagedContentVersionMeta);
}
