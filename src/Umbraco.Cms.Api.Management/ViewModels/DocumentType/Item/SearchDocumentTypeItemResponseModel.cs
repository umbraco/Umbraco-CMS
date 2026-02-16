using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType.Item;

public class SearchDocumentTypeItemResponseModel : DocumentTypeItemResponseModel
{
    public IEnumerable<SearchResultAncestorModel> Ancestors { get; set; } = [];
}
