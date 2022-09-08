namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A value indicating the result of an operation.
/// </summary>
public enum OperationResultType : byte
{
    // all "ResultType" enum's must be byte-based, and declare Failed = 128, and declare
    // every failure codes as >128 - see OperationResult and OperationResultType for details.

    /// <summary>
    ///     The operation was successful.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     The operation failed.
    /// </summary>
    /// <remarks>All values above this value indicate a failure.</remarks>
    Failed = 128,

    /// <summary>
    ///     The operation could not complete because of invalid preconditions (eg creating a reference
    ///     to an item that does not exist).
    /// </summary>
    FailedCannot = Failed | 2,

    /// <summary>
    ///     The operation has been cancelled by an event handler.
    /// </summary>
    FailedCancelledByEvent = Failed | 4,

    /// <summary>
    ///     The operation could not complete due to an exception.
    /// </summary>
    FailedExceptionThrown = Failed | 5,

    /// <summary>
    ///     No operation has been executed because it was not needed (eg deleting an item that doesn't exist).
    /// </summary>
    NoOperation = Failed | 6, // TODO: shouldn't it be a success?

    // TODO: In the future, we might need to add more operations statuses, potentially like 'FailedByPermissions', etc...
}
