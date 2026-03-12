using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

/// <summary>
///     Published when one or more migration plans has been executed.
/// </summary>
/// <remarks>
/// <para>
///     Each migration plan may or may not have succeeded, signaled by the Successful property.
/// </para>
/// <para>
///     A failed migration plan may have partially completed, in which case the successful transition are located in the CompletedTransitions collection.
/// </para>
/// </remarks>
public class MigrationPlansExecutedNotification : INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationPlansExecutedNotification"/> class using the specified collection of executed migration plans.
    /// </summary>
    /// <param name="executedPlans">A read-only list containing the executed migration plans.</param>
    public MigrationPlansExecutedNotification(IReadOnlyList<ExecutedMigrationPlan> executedPlans)
        => ExecutedPlans = executedPlans;

    /// <summary>Gets the list of executed migration plans.</summary>
    public IReadOnlyList<ExecutedMigrationPlan> ExecutedPlans { get; }
}
