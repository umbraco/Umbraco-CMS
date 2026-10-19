namespace Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;

/// <summary>
/// One culture/segment variant's indexed fields, as returned by the backoffice "Show Fields" feature.
/// </summary>
public class IndexDocumentViewModel
{
    /// <summary>
    /// Gets or sets the indexed fields for this document variant.
    /// </summary>
    public required IReadOnlyCollection<FieldViewModel> Fields { get; set; }
}
