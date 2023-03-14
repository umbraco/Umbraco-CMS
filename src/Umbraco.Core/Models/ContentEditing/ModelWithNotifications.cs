using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     A generic model supporting notifications, this is useful for returning any model type to include notifications from
///     api controllers
/// </summary>
/// <typeparam name="T"></typeparam>
[DataContract(Name = "model", Namespace = "")]
public class ModelWithNotifications<T> : INotificationModel
{
    public ModelWithNotifications(T value)
    {
        Value = value;
        Notifications = new List<BackOfficeNotification>();
    }

    /// <summary>
    ///     The generic value
    /// </summary>
    [DataMember(Name = "value")]
    public T Value { get; private set; }

    /// <summary>
    ///     The notifications
    /// </summary>
    [DataMember(Name = "notifications")]
    public List<BackOfficeNotification> Notifications { get; private set; }
}
