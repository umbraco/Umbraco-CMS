namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A status type of the result of moving an item
/// </summary>
/// <remarks>
///     Anything less than 10 = Success!
/// </remarks>
public enum MoveOperationStatusType : byte
{
    /// <summary>
    ///     The move was successful.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     The parent being moved to doesn't exist
    /// </summary>
    FailedParentNotFound = 13,

    /// <summary>
    ///     The move action has been cancelled by an event handler
    /// </summary>
    FailedCancelledByEvent = 14,

    /// <summary>
    ///     Trying to move an item to an invalid path (i.e. a child of itself)
    /// </summary>
    FailedNotAllowedByPath = 15,
}
