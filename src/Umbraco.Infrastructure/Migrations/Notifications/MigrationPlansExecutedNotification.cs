using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

/// <summary>
///     Published when one or more migration plans have been successfully executed.
/// </summary>
public class MigrationPlansExecutedNotification : INotification
{
    public MigrationPlansExecutedNotification(IReadOnlyList<ExecutedMigrationPlan> executedPlans)
        => ExecutedPlans = executedPlans;

    public IReadOnlyList<ExecutedMigrationPlan> ExecutedPlans { get; }
}
