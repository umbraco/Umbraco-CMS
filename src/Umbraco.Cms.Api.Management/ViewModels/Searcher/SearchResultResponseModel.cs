using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

/// <summary>
/// Represents a response model containing search results in the Umbraco CMS Management API.
/// </summary>
public class SearchResultResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the search result.
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the relevance score of the search result.</summary>
    public float Score { get; set; }

    /// <summary>
    /// Gets the count of fields in the search result.
    /// </summary>
    public int FieldCount => Fields.Count();

    /// <summary>
    /// Gets or sets the collection of field presentation models associated with the search result.
    /// </summary>
    public IEnumerable<FieldPresentationModel> Fields { get; set; } = Enumerable.Empty<FieldPresentationModel>();
}
