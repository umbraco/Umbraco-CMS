using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "auditLog", Namespace = "")]
    public class AuditLog
    {
        [DataMember(Name = "userId", IsRequired = true)]
        public int UserId { get; set; }

        [DataMember(Name = "nodeId", IsRequired = true)]
        public int NodeId { get; set; }

        [DataMember(Name = "timestamp", IsRequired = true)]
        public DateTime Timestamp { get; set; }

        [DataMember(Name = "logType", IsRequired = true)]
        public AuditLogType LogType { get; set; }

        [DataMember(Name = "comment", IsRequired = true)]
        public string Comment { get; set; }
    }
}