using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

[DataContract(Name = "dictionaryImportModel")]
public class DictionaryImportViewModel
{
    [DataMember(Name = "dictionaryItems")]
    public List<DictionaryPreviewImportModel>? DictionaryItems { get; set; }

    [DataMember(Name = "tempFileName")]
    public string? TempFileName { get; set; }
}
