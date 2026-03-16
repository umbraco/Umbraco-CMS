using System.Globalization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The identity user used for the member
/// </summary>
public class MemberIdentityUser : UmbracoIdentityUser
{
    // Custom comparer for enumerables
    private static readonly DelegateEqualityComparer<IReadOnlyCollection<IReadOnlyUserGroup>> _groupsComparer = new(
        (groups, enumerable) =>
            groups?.Select(x => x.Alias).UnsortedSequenceEqual(enumerable?.Select(x => x.Alias)) ?? false,
        groups => groups.GetHashCode());

    private string? _comments;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityUser" /> class.
    /// </summary>
    public MemberIdentityUser(int userId) =>

        // use the property setters - they do more than just setting a field
        Id = UserIdToString(userId);

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberIdentityUser"/> class with default values.
    /// </summary>
    public MemberIdentityUser()
    {
    }

    /// <summary>
    ///     Gets or sets the member's comments
    /// </summary>
    public string? Comments
    {
        get => _comments;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _comments, nameof(Comments));
    }

    /// <summary>
    /// Gets or sets the date and time when the member account was most recently locked out.
    /// This value is set when the <see cref="IsLockedOut"/> flag is updated.
    /// </summary>
    /// <remarks>No change tracking because the persisted value is only set with the IsLockedOut flag</remarks>
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member identity user was created.
    /// </summary>
    /// <remarks>No change tracking because the persisted value is readonly</remarks>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier key for the member identity user.
    /// </summary>
    /// <remarks>No change tracking because the persisted value is readonly</remarks>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the member type
    /// </summary>
    public string? MemberTypeAlias { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this member exists only as an external identity
    ///     (backed by the lightweight umbracoExternalMember table, not the content system).
    /// </summary>
    /// <remarks>No change tracking because this is a routing flag set at load time.</remarks>
    public bool IsExternalOnly { get; set; }

    /// <summary>
    ///     Gets or sets arbitrary profile data as a JSON string.
    ///     Only used for external-only members. For content-based members, profile data
    ///     lives in content properties and this value is null.
    /// </summary>
    /// <remarks>No change tracking because this is managed by IExternalMemberService.</remarks>
    public string? ProfileData { get; set; }

    /// <summary>
    ///     Deserialises the <see cref="ProfileData"/> JSON string into a typed object.
    /// </summary>
    /// <typeparam name="T">The type to deserialise to.</typeparam>
    /// <param name="options">Optional JSON serializer options. If null, the default <see cref="System.Text.Json.JsonSerializer"/> options are used.</param>
    /// <returns>The deserialised object, or <c>default</c> if <see cref="ProfileData"/> is null or empty.</returns>
    public T? GetProfileData<T>(System.Text.Json.JsonSerializerOptions? options = null) where T : class =>
        string.IsNullOrEmpty(ProfileData) ? default : System.Text.Json.JsonSerializer.Deserialize<T>(ProfileData, options);

    /// <summary>
    ///     Creates a new <see cref="MemberIdentityUser"/> instance without assigning an identity.
    /// </summary>
    /// <param name="username">The username for the new member.</param>
    /// <param name="email">The email address for the new member.</param>
    /// <param name="memberTypeAlias">The alias of the member type.</param>
    /// <param name="isApproved">True if the member is approved; otherwise, false.</param>
    /// <param name="name">The display name of the member (optional).</param>
    /// <param name="key">The unique identifier key for the member (optional).</param>
    /// <returns>A <see cref="MemberIdentityUser"/> instance with no identity assigned (<c>HasIdentity</c> is false).</returns>
    public static MemberIdentityUser CreateNew(string username, string email, string memberTypeAlias, bool isApproved, string? name = null, Guid? key = null)
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
        user.Key = key ?? user.Key;
        user.HasIdentity = false;
        user.Name = name;
        user.EnableChangeTracking();
        return user;
    }

    private static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));

    // TODO: Should we support custom member properties for persistence/retrieval?
}
