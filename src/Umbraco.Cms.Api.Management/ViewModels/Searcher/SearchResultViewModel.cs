namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class SearchResultViewModel
{
    public string Id { get; set; } = null!;

    public float Score { get; set; }

    public int FieldCount => Fields.Count();

    public IEnumerable<FieldViewModel> Fields { get; set; } = null!;
}
