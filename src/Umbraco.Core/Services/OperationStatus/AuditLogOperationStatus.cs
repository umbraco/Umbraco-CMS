namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an audit log operation.
/// </summary>
public enum AuditLogOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified user was not found.
    /// </summary>
    UserNotFound,
}
