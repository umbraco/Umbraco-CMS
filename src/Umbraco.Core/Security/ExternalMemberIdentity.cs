// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents a lightweight identity model for an external-only member
///     that is not backed by the content system.
/// </summary>
/// <remarks>
///     External-only members are stored in the <c>umbracoExternalMember</c> table
///     and do not have content properties, content types, or tree structure.
///     Profile data is stored as a JSON string.
/// </remarks>
public class ExternalMemberIdentity
{
    /// <summary>
    ///     Gets or sets the database identity of the external member.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier key for the external member.
    /// </summary>
    public Guid Key { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets the email address of the external member.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username of the external member.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the external member.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the external member is approved.
    /// </summary>
    public bool IsApproved { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the external member is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the last login.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the last lockout.
    /// </summary>
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the external member was created.
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets the security stamp used for concurrency validation.
    /// </summary>
    public string? SecurityStamp { get; set; }

    /// <summary>
    ///     Gets or sets arbitrary profile data as a JSON string.
    /// </summary>
    public string? ProfileData { get; set; }

    /// <summary>
    ///     Deserializes the <see cref="ProfileData"/> JSON string into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized object, or <c>null</c> if <see cref="ProfileData"/> is null or empty.</returns>
    public T? GetProfileData<T>()
    {
        if (string.IsNullOrEmpty(ProfileData))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(ProfileData);
    }

    /// <summary>
    ///     Serializes the specified value and stores it as the <see cref="ProfileData"/> JSON string.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="value">The value to serialize and store.</param>
    public void SetProfileData<T>(T value) => ProfileData = JsonSerializer.Serialize(value);
}
