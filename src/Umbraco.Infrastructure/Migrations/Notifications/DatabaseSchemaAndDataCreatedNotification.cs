using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

/// <summary>
/// Notification that is raised after the database schema and initial data have been successfully created.
/// </summary>
public class DatabaseSchemaAndDataCreatedNotification : INotification
{
    /// <summary>
    /// Gets a value indicating whether the database schema or data requires an upgrade.
    /// </summary>
    public bool RequiresUpgrade { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSchemaAndDataCreatedNotification"/> class, indicating whether the database schema and data creation process requires an upgrade.
    /// </summary>
    /// <param name="requiresUpgrade">A <see cref="bool"/> value indicating whether an upgrade is required after creating the database schema and data.</param>
    public DatabaseSchemaAndDataCreatedNotification(bool requiresUpgrade)
        => RequiresUpgrade = requiresUpgrade;
}
