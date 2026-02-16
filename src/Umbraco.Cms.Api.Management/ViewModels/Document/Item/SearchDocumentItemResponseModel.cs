using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Item;

public class SearchDocumentItemResponseModel : DocumentItemResponseModel
{
    public IEnumerable<SearchResultAncestorModel> Ancestors { get; set; } = [];
}
