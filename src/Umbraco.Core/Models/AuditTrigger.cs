namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the trigger context for an audit log entry, identifying the software or package
///     and the operation that initiated the audited action.
/// </summary>
/// <param name="Source">The software or package that triggered the action (e.g. "Core", "Umbraco.Workflow").</param>
/// <param name="Operation">The operation that triggered the action (e.g. "ScheduledPublish", "Rollback", "FinalApproval").</param>
/// <param name="SuppressForAuditTypes">
///     Optional audit types for which the trigger is considered redundant and will be omitted from the recorded entry.
///     For example, a rollback trigger is redundant on a <see cref="AuditType.RollBack" /> entry because the type itself conveys the same information,
///     but remains meaningful on the nested <see cref="AuditType.Save" /> entry that the rollback produces.
/// </param>
public sealed record AuditTrigger(string Source, string Operation, IReadOnlyCollection<AuditType>? SuppressForAuditTypes = null);
