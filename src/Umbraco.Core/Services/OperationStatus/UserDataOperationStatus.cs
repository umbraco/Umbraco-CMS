namespace Umbraco.Cms.Core.Services.OperationStatus;

// FIXME: Move all authorization statuses to <see cref="UserGroupAuthorizationStatus"/>

/// <summary>
///     Represents the status of a user data operation.
/// </summary>
public enum UserDataOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the user data could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation failed because the user could not be found.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The operation failed because user data with the same identifier already exists.
    /// </summary>
    AlreadyExists
}
