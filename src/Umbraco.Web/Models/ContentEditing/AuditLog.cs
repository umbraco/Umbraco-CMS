using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{

    [DataContract(Name = "auditLog", Namespace = "")]
    public class AuditLog
    {
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "userAvatars")]
        public string[] UserAvatars { get; set; }        

        [DataMember(Name = "nodeId")]
        public int NodeId { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "logType")]
        public AuditType LogType { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }
    }
}
