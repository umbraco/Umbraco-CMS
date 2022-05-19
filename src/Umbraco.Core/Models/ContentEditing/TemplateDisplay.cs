using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "template", Namespace = "")]
public class TemplateDisplay : INotificationModel
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [Required]
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [Required]
    [DataMember(Name = "alias")]
    public string Alias { get; set; } = string.Empty;

    [DataMember(Name = "key")]
    public Guid Key { get; set; }

    [DataMember(Name = "content")]
    public string? Content { get; set; }

    [DataMember(Name = "path")]
    public string? Path { get; set; }

    [DataMember(Name = "virtualPath")]
    public string? VirtualPath { get; set; }

    [DataMember(Name = "masterTemplateAlias")]
    public string? MasterTemplateAlias { get; set; }

    [DataMember(Name = "isMasterTemplate")]
    public bool IsMasterTemplate { get; set; }

    /// <summary>
    ///     This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification>? Notifications { get; private set; }
}
