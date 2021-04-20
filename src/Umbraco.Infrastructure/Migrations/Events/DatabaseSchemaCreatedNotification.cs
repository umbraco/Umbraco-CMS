using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Infrastructure.Migrations.Events
{
    internal class DatabaseSchemaCreatedNotification : StatefulNotification
    {
        public DatabaseSchemaCreatedNotification(EventMessages eventMessages) => EventMessages = eventMessages;

        public EventMessages EventMessages { get; }

    }
}
