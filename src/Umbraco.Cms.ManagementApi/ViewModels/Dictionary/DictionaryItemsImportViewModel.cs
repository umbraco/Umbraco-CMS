using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

[DataContract(Name = "dictionaryPreviewImportModel")]
public class DictionaryItemsImportViewModel
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "level")]
    public int Level { get; set; }
}
