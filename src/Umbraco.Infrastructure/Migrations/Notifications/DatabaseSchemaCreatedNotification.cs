using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

internal sealed class DatabaseSchemaCreatedNotification : StatefulNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSchemaCreatedNotification"/> class with the specified event messages.
    /// </summary>
    /// <param name="eventMessages">The event messages related to the creation of the database schema.</param>
    public DatabaseSchemaCreatedNotification(EventMessages eventMessages) => EventMessages = eventMessages;

    /// <summary>
    /// Gets the collection of event messages for the database schema creation notification.
    /// </summary>
    public EventMessages EventMessages { get; }
}
