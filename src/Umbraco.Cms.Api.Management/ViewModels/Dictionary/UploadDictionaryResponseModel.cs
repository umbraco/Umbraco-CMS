namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class UploadDictionaryResponseModel
{
    public IEnumerable<ImportDictionaryItemsPresentationModel> DictionaryItems { get; set; } = Array.Empty<ImportDictionaryItemsPresentationModel>();

    public string? FileName { get; set; }
}
