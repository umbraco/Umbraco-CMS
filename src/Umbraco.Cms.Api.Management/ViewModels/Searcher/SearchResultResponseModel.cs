using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class SearchResultResponseModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    public float Score { get; set; }

    public int FieldCount => Fields.Count();

    public IEnumerable<FieldPresentationModel> Fields { get; set; } = Enumerable.Empty<FieldPresentationModel>();
}
