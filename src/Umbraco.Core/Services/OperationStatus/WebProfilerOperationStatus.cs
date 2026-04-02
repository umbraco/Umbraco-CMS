namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a web profiler operation.
/// </summary>
public enum WebProfilerOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the executing user could not be found.
    /// </summary>
    ExecutingUserNotFound
}
