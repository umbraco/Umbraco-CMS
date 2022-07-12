using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models;

[DataContract(Name = "contentTypeImportModel")]
public class ContentTypeImportModel : INotificationModel
{
    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "tempFileName")]
    public string? TempFileName { get; set; }

    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; } = new();
}
