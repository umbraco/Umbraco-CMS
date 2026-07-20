namespace Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;

public class DocumentViewModel
{
    public required Guid Key { get; set; }

    public required IEnumerable<IndexDocumentViewModel> Documents { get; set; }
}
