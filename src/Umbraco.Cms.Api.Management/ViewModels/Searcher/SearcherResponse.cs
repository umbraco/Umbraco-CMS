using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

/// <summary>
/// Represents the view model for the response returned by a searcher operation in the Umbraco CMS Management API.
/// </summary>
public class SearcherResponse
{
    /// <summary>
    /// Gets or sets the name of the searcher.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}
