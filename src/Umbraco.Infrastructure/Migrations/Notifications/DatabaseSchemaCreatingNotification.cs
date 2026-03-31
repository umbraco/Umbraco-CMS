using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

internal sealed class DatabaseSchemaCreatingNotification : CancelableNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSchemaCreatingNotification"/> class with the specified event messages.
    /// </summary>
    /// <param name="messages">The <see cref="EventMessages"/> associated with the database schema creation event.</param>
    public DatabaseSchemaCreatingNotification(EventMessages messages)
        : base(messages)
    {
    }
}
