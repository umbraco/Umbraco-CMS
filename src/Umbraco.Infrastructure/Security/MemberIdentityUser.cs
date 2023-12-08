using System.Globalization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The identity user used for the member
/// </summary>
public class MemberIdentityUser : UmbracoIdentityUser
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityUser" /> class.
    /// </summary>
    public MemberIdentityUser(int userId) =>

        // use the property setters - they do more than just setting a field
        Id = UserIdToString(userId);

    public MemberIdentityUser()
    {
    }

    // No change tracking because the persisted value is only set with the IsLockedOut flag
    public DateTime? LastLockoutDateUtc { get; set; }

    // No change tracking because the persisted value is readonly
    public DateTime CreatedDateUtc { get; set; }

    // No change tracking because the persisted value is readonly
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the member type
    /// </summary>
    public string? MemberTypeAlias { get; set; }

    /// <summary>
    ///     Used to construct a new instance without an identity
    /// </summary>
    public static MemberIdentityUser CreateNew(string username, string email, string memberTypeAlias, bool isApproved,
        string? name = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        var user = new MemberIdentityUser();
        user.DisableChangeTracking();
        user.UserName = username;
        user.Email = email;
        user.MemberTypeAlias = memberTypeAlias;
        user.IsApproved = isApproved;
        user.Id = null!;
        user.HasIdentity = false;
        user.Name = name;
        user.EnableChangeTracking();
        return user;
    }

    private static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));

    // TODO: Should we support custom member properties for persistence/retrieval?
}
