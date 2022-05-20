using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    [DataContract(Name = "dictionaryImportModel")]
    public class DictionaryImportModel
    {
        [DataMember(Name = "dictionaryItems")]
        public List<DictionaryPreviewImportModel>? DictionaryItems { get; set; }

        [DataMember(Name = "tempFileName")]
        public string? TempFileName { get; set; }
    }
}
