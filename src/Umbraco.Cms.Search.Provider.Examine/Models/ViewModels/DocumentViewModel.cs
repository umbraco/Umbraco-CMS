namespace Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;

/// <summary>
/// The indexed documents for a content item, as returned by the backoffice "Show Fields" feature.
/// </summary>
public class DocumentViewModel
{
    /// <summary>
    /// Gets or sets the key of the content item.
    /// </summary>
    public required Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the indexed documents for the content item, one per culture/segment variant.
    /// </summary>
    public required IEnumerable<IndexDocumentViewModel> Documents { get; set; }
}
