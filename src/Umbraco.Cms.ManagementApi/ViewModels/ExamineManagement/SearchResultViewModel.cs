namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class SearchResultViewModel
{
    public string? Id { get; set; }

    public float Score { get; set; }

    public int FieldCount => Fields?.Count() ?? 0;

    public IEnumerable<FieldViewModel>? Fields { get; set; }
}
