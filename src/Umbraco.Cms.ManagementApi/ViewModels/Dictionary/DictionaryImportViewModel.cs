namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

public class DictionaryImportViewModel
{
    public List<DictionaryItemsImportViewModel> DictionaryItems { get; set; } = null!;

    public string? TempFileName { get; set; }
}
