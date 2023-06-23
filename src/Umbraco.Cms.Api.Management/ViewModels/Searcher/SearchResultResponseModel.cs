namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

public class SearchResultResponseModel
{
    public string Id { get; set; } = null!;

    public float Score { get; set; }

    public int FieldCount => Fields.Count();

    public IEnumerable<FieldPresentationModel> Fields { get; set; } = null!;
}
