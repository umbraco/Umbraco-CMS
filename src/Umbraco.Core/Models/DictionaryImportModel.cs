using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    [DataContract(Name = "dictionaryImportModel")]
    public class DictionaryImportModel
    {
        [DataMember(Name = "dictionaryItems")]
        public List<string> DictionaryItems { get; set; }

        [DataMember(Name = "tempFileName")]
        public string TempFileName { get; set; }
    }
}
