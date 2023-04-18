using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.Migrations.Notifications;

/// <summary>
///     Published the umbraco migration plan has been executed.
/// </summary>
/// <remarks>
/// <para>
///     The migration plan may or may not have succeeded, signaled by the Successful property.
/// </para>
/// <para>
///     A failed migration plan may have partially completed, in which case the successful transition are located in the CompletedTransitions collection.
/// </para>
/// </remarks>
public class UmbracoPlanExecutedNotification : INotification
{
    public required ExecutedMigrationPlan ExecutedPlan { get; init; }
}
