using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    [DataContract(Name = "dictionaryPreviewImportModel")]
    public class DictionaryPreviewImportModel
    {
        [DataMember(Name = "name")]
        public string? Name { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }
    }
}
