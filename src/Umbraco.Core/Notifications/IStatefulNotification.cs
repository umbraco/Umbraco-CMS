namespace Umbraco.Cms.Core.Notifications;

public interface IStatefulNotification : INotification
{
    IDictionary<string, object?> State { get; set; }
}
