using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the base contract for <see cref="IMember" /> and <see cref="IUser" />
/// </summary>
public interface IMembershipUser : IEntity
{
    /// <summary>
    ///     Gets or sets the username for the membership user.
    /// </summary>
    string Username { get; set; }

    /// <summary>
    ///     Gets or sets the email address for the membership user.
    /// </summary>
    string Email { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the email was confirmed.
    /// </summary>
    DateTime? EmailConfirmedDate { get; set; }

    /// <summary>
    ///     Gets or sets the raw password value
    /// </summary>
    string? RawPasswordValue { get; set; }

    /// <summary>
    ///     The user's specific password config (i.e. algorithm type, etc...)
    /// </summary>
    string? PasswordConfiguration { get; set; }

    /// <summary>
    ///     Gets or sets any comments associated with the membership user.
    /// </summary>
    string? Comments { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the membership user is approved.
    /// </summary>
    bool IsApproved { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the membership user is locked out.
    /// </summary>
    bool IsLockedOut { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the user's last login.
    /// </summary>
    DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the password was last changed.
    /// </summary>
    DateTime? LastPasswordChangeDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the user was last locked out.
    /// </summary>
    DateTime? LastLockoutDate { get; set; }

    /// <summary>
    ///     Gets or sets the number of failed password attempts.
    ///     This is the number of times the password was entered incorrectly upon login.
    /// </summary>
    /// <remarks>
    ///     Alias: umbracoMemberFailedPasswordAttempts
    ///     Part of the standard properties collection.
    /// </remarks>
    int FailedPasswordAttempts { get; set; }

    /// <summary>
    ///     Gets or sets the security stamp used by ASP.NET Identity
    /// </summary>
    string? SecurityStamp { get; set; }

    // object ProfileId { get; set; }
    // IEnumerable<object> Groups { get; set; }
}
