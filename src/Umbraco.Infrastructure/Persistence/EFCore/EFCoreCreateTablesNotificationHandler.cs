using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// Notification handler that creates EF Core database tables after schema creation or unattended install.
/// </summary>
public class EFCoreCreateTablesNotificationHandler : INotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification>, INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IEFCoreMigrationExecutor _iefCoreMigrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreCreateTablesNotificationHandler"/> class.
    /// </summary>
    /// <param name="iefCoreMigrationExecutor">The EF Core migration executor.</param>
    public EFCoreCreateTablesNotificationHandler(IEFCoreMigrationExecutor iefCoreMigrationExecutor)
    {
        _iefCoreMigrationExecutor = iefCoreMigrationExecutor;
    }

    /// <inheritdoc />
    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        await HandleAsync();
    }

    /// <inheritdoc />
    public async Task HandleAsync(DatabaseSchemaAndDataCreatedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RequiresUpgrade is false)
        {
            await HandleAsync();
        }
    }

    private async Task HandleAsync()
    {
        await _iefCoreMigrationExecutor.ExecuteAllMigrationsAsync();
    }
}
