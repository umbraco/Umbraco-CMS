namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published member.
/// </summary>
/// <remarks>
///     Extends <see cref="IPublishedContent"/> with member-specific properties such as email,
///     username, and account status information.
/// </remarks>
public interface IPublishedMember : IPublishedContent
{
    /// <summary>
    ///     Gets the email address of the member.
    /// </summary>
    public string Email { get; }

    /// <summary>
    ///     Gets the username of the member.
    /// </summary>
    public string UserName { get; }

    /// <summary>
    ///     Gets the comments associated with the member account.
    /// </summary>
    public string? Comments { get; }

    /// <summary>
    ///     Gets a value indicating whether the member account is approved.
    /// </summary>
    public bool IsApproved { get; }

    /// <summary>
    ///     Gets a value indicating whether the member account is locked out.
    /// </summary>
    public bool IsLockedOut { get; }

    /// <summary>
    ///     Gets the date and time when the member account was last locked out.
    /// </summary>
    public DateTime? LastLockoutDate { get; }

    /// <summary>
    ///     Gets the date and time when the member account was created.
    /// </summary>
    public DateTime CreationDate { get; }

    /// <summary>
    ///     Gets the date and time of the member's last login.
    /// </summary>
    public DateTime? LastLoginDate { get; }

    /// <summary>
    ///     Gets the date and time when the member's password was last changed.
    /// </summary>
    public DateTime? LastPasswordChangedDate { get; }
}
