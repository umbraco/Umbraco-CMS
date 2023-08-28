using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

public class DatabaseSchemaAndDataCreatedNotification : INotification
{
    public bool RequiresUpgrade { get; }

    public DatabaseSchemaAndDataCreatedNotification(bool requiresUpgrade)
        => RequiresUpgrade = requiresUpgrade;
}
