namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Specifies the field by which to order user query results.
/// </summary>
public enum UserOrder
{
    /// <summary>
    ///     Order by username.
    /// </summary>
    UserName,

    /// <summary>
    ///     Order by preferred language.
    /// </summary>
    Language,

    /// <summary>
    ///     Order by display name.
    /// </summary>
    Name,

    /// <summary>
    ///     Order by email address.
    /// </summary>
    Email,

    /// <summary>
    ///     Order by user identifier.
    /// </summary>
    Id,

    /// <summary>
    ///     Order by creation date.
    /// </summary>
    CreateDate,

    /// <summary>
    ///     Order by last update date.
    /// </summary>
    UpdateDate,

    /// <summary>
    ///     Order by approval status.
    /// </summary>
    IsApproved,

    /// <summary>
    ///     Order by locked out status.
    /// </summary>
    IsLockedOut,

    /// <summary>
    ///     Order by last login date.
    /// </summary>
    LastLoginDate,
}
