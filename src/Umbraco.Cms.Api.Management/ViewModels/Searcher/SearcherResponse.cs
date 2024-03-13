using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class SearcherResponse
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
