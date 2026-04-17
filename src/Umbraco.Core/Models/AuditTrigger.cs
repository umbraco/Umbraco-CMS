namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the trigger context for an audit log entry, identifying the software or package
///     and the operation that initiated the audited action.
/// </summary>
/// <param name="Source">The software or package that triggered the action (e.g. "Core", "Umbraco.Workflow").</param>
/// <param name="Operation">The operation that triggered the action (e.g. "ScheduledPublish", "Rollback", "FinalApproval").</param>
public sealed record AuditTrigger(string Source, string Operation);
