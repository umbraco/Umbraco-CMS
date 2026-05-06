using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
/// Data transfer object representing a user in the Umbraco CMS persistence layer.
/// </summary>
[EntityTypeConfiguration(typeof(UserDtoConfiguration))]
public class UserDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = "key";

    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the unique GUID key that identifies the user.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is prevented from accessing the console.
    /// </summary>
    public bool NoConsole { get; set; }

    /// <summary>
    /// Gets or sets the unique user name associated with the user.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username used for logging in to the user account.
    /// </summary>
    public string? Login { get; set; }

    /// <summary>
    /// Gets or sets the user's hashed password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a JSON structure of how the password has been created (i.e hash algorithm, iterations).
    /// </summary>
    public string? PasswordConfig { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ISO language code representing the user's preferred language.
    /// </summary>
    public string? UserLanguage { get; set; }

    /// <summary>
    /// Gets or sets the security stamp token for the user.
    /// </summary>
    public string? SecurityStampToken { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    public int? FailedLoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last locked out.
    /// </summary>
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user's password was last changed.
    /// </summary>
    public DateTime? LastPasswordChangeDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user last logged in.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user's email was confirmed.
    /// </summary>
    public DateTime? EmailConfirmedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the user was invited.
    /// </summary>
    public DateTime? InvitedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the user was last updated.
    /// </summary>
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the kind of the user, typically used to distinguish between different user types or roles.
    /// </summary>
    public short Kind { get; set; }

    /// <summary>
    /// Gets or sets the media file system relative path of the user's custom avatar if uploaded.
    /// </summary>
    public string? Avatar { get; set; }
}
