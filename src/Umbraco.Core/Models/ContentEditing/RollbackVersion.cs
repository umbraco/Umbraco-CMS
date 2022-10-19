using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "rollbackVersion", Namespace = "")]
public class RollbackVersion
{
    [DataMember(Name = "versionId")]
    public int VersionId { get; set; }

    [DataMember(Name = "versionDate")]
    public DateTime? VersionDate { get; set; }

    [DataMember(Name = "versionAuthorId")]
    public int VersionAuthorId { get; set; }

    [DataMember(Name = "versionAuthorName")]
    public string? VersionAuthorName { get; set; }
}
