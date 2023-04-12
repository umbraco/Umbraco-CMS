namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryImportViewModel
{
    public List<DictionaryItemsImportViewModel> DictionaryItems { get; set; } = null!;

    public string? TempFileName { get; set; }
}
