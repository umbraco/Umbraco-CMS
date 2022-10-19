using System.Globalization;
using Umbraco.Cms.Core.Models;
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
    public MemberIdentityUser()
    {
        Properties = new PropertyCollection();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityUser" /> class.
    /// </summary>
    public MemberIdentityUser(int userId)
    {
        Id = UserIdToString(userId);
        Properties = new PropertyCollection();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityUser" /> class.
    /// </summary>
    public MemberIdentityUser(IPropertyCollection properties)
    {
        Properties = properties;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityUser" /> class.
    /// </summary>
    public MemberIdentityUser(int userId, IPropertyCollection properties)
    {
        Id = UserIdToString(userId);
        Properties = properties;
    }

    /// <summary>
    ///     Gets or sets the member's comments
    /// </summary>
    public string? Comments
    {
        get => _comments;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _comments, nameof(Comments));
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
    ///     Gets or sets the properties of the member
    /// </summary>
    public IPropertyCollection Properties { get; set; }

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
        user.Id = null!;
        user.Name = name;
        user.Email = email;
        user.UserName = username;
        user.MemberTypeAlias = memberTypeAlias;
        user.IsApproved = isApproved;
        user.HasIdentity = false;
        user.EnableChangeTracking();
        return user;
    }

    private static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));
}
