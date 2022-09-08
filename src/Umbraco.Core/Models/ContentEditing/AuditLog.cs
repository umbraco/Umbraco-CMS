using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "auditLog", Namespace = "")]
public class AuditLog
{
    [DataMember(Name = "userId")]
    public int UserId { get; set; }

    [DataMember(Name = "userName")]
    public string? UserName { get; set; }

    [DataMember(Name = "userAvatars")]
    public string[]? UserAvatars { get; set; }

    [DataMember(Name = "nodeId")]
    public int NodeId { get; set; }

    [DataMember(Name = "timestamp")]
    public DateTime Timestamp { get; set; }

    [DataMember(Name = "logType")]
    public string? LogType { get; set; }

    [DataMember(Name = "entityType")]
    public string? EntityType { get; set; }

    [DataMember(Name = "comment")]
    public string? Comment { get; set; }

    [DataMember(Name = "parameters")]
    public string? Parameters { get; set; }
}
