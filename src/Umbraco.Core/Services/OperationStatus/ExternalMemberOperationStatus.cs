// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an external member operation.
/// </summary>
public enum ExternalMemberOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the external member could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because a member with the same username already exists.
    /// </summary>
    DuplicateUsername,

    /// <summary>
    ///     The operation failed because a member with the same email already exists.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    ///     The operation is not yet implemented.
    /// </summary>
    NotImplemented,
}
