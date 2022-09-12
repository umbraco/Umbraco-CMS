using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

[DataContract(Name = "dictionaryImportModel")]
public class DictionaryImportViewModel
{
    [DataMember(Name = "dictionaryItems")]
    public List<DictionaryItemsImportViewModel>? DictionaryItems { get; set; }

    [DataMember(Name = "tempFileName")]
    public string? TempFileName { get; set; }
}
