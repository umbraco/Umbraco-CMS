using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models
{
    [DataContract(Name = "dictionaryImportModel")]
    public class DictionaryImportModel : INotificationModel
    {
        [DataMember(Name = "dictionaryItems")]
        public List<string> DictionaryItems { get; set; }

        [DataMember(Name = "notifications")]
        public List<BackOfficeNotification> Notifications { get; } = new List<BackOfficeNotification>();

        [DataMember(Name = "tempFileName")]
        public string TempFileName { get; set; }
    }
}
