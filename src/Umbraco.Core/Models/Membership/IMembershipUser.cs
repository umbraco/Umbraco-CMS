using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the base contract for <see cref="IMember" /> and <see cref="IUser" />
/// </summary>
public interface IMembershipUser : IEntity
{
    string Username { get; set; }

    string Email { get; set; }

    DateTime? EmailConfirmedDate { get; set; }

    /// <summary>
    ///     Gets or sets the raw password value
    /// </summary>
    string? RawPasswordValue { get; set; }

    /// <summary>
    ///     The user's specific password config (i.e. algorithm type, etc...)
    /// </summary>
    string? PasswordConfiguration { get; set; }

    string? Comments { get; set; }

    bool IsApproved { get; set; }

    bool IsLockedOut { get; set; }

    DateTime? LastLoginDate { get; set; }

    DateTime? LastPasswordChangeDate { get; set; }

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
