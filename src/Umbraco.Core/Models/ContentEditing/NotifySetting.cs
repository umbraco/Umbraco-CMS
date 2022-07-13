using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "notifySetting", Namespace = "")]
public class NotifySetting
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "checked")]
    public bool Checked { get; set; }

    /// <summary>
    ///     The letter from the IAction
    /// </summary>
    [DataMember(Name = "notifyCode")]
    public string? NotifyCode { get; set; }
}
