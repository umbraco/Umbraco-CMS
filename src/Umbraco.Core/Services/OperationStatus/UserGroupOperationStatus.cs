namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a user group operation.
/// </summary>
public enum UserGroupOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the user group could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation failed because the user could not be found.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The operation failed because a user group with the same identifier already exists.
    /// </summary>
    AlreadyExists,

    /// <summary>
    ///     The operation failed because a user group with the same alias already exists.
    /// </summary>
    DuplicateAlias,

    /// <summary>
    ///     The operation failed because the specified user is missing.
    /// </summary>
    MissingUser,

    /// <summary>
    ///     The operation failed because system user groups cannot be deleted.
    /// </summary>
    CanNotDeleteIsSystemUserGroup,

    /// <summary>
    ///     The operation failed because the alias of a system user group cannot be updated.
    /// </summary>
    CanNotUpdateAliasIsSystemUserGroup,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because the specified media start node key could not be found.
    /// </summary>
    MediaStartNodeKeyNotFound,

    /// <summary>
    ///     The operation failed because the specified document start node key could not be found.
    /// </summary>
    DocumentStartNodeKeyNotFound,

    /// <summary>
    ///     The operation failed because the specified document permission key could not be found.
    /// </summary>
    DocumentPermissionKeyNotFound,

    /// <summary>
    ///     The operation failed because the specified document type permission key could not be found.
    /// </summary>
    DocumentTypePermissionKeyNotFound,

    /// <summary>
    ///     The operation failed because the specified language could not be found.
    /// </summary>
    LanguageNotFound,

    /// <summary>
    ///     The operation failed because the user group name exceeds the maximum allowed length.
    /// </summary>
    NameTooLong,

    /// <summary>
    ///     The operation failed because the user group alias exceeds the maximum allowed length.
    /// </summary>
    AliasTooLong,

    /// <summary>
    ///     The operation failed because the user group name is required but was not provided.
    /// </summary>
    MissingName,

    /// <summary>
    ///     The operation failed because the current user is not authorized to perform this action.
    /// </summary>
    Unauthorized,

    /// <summary>
    ///     The operation failed because the admin group cannot be empty.
    /// </summary>
    AdminGroupCannotBeEmpty,

    /// <summary>
    ///     The operation failed because the user is not a member of the specified group.
    /// </summary>
    UserNotInGroup,
}
