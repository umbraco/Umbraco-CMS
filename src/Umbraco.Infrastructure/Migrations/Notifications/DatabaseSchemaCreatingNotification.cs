using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

internal sealed class DatabaseSchemaCreatingNotification : CancelableNotification
{
    public DatabaseSchemaCreatingNotification(EventMessages messages)
        : base(messages)
    {
    }
}
