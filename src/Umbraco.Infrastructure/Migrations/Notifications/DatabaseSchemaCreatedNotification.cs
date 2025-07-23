using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

internal sealed class DatabaseSchemaCreatedNotification : StatefulNotification
{
    public DatabaseSchemaCreatedNotification(EventMessages eventMessages) => EventMessages = eventMessages;

    public EventMessages EventMessages { get; }
}
