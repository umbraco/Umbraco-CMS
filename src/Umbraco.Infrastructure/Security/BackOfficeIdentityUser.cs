using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The identity user used for the back office
/// </summary>
public class BackOfficeIdentityUser : UmbracoIdentityUser
{
    private static readonly DelegateEqualityComparer<int[]> _startIdsComparer = new(
        (groups, enumerable) => groups.UnsortedSequenceEqual(enumerable),
        groups => groups.GetHashCode());

    private string[]? _allowedSections;
    private string _culture;
    private IReadOnlyCollection<IReadOnlyUserGroup> _groups = null!;
    private DateTime? _inviteDateUtc;
    private int[] _startContentIds;
    private int[] _startMediaIds;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeIdentityUser" /> class.
    /// </summary>
    public BackOfficeIdentityUser(GlobalSettings globalSettings, int userId, IEnumerable<IReadOnlyUserGroup> groups)
        : this(globalSettings, groups.ToArray()) =>

        // use the property setters - they do more than just setting a field
        Id = UserIdToString(userId);

    private BackOfficeIdentityUser(GlobalSettings globalSettings, IReadOnlyCollection<IReadOnlyUserGroup> groups)
    {
        _startMediaIds = Array.Empty<int>();
        _startContentIds = Array.Empty<int>();
        _allowedSections = Array.Empty<string>();
        _culture = globalSettings.DefaultUILanguage;

        SetGroups(groups);
    }

    public int[]? CalculatedMediaStartNodeIds { get; set; }

    public int[]? CalculatedContentStartNodeIds { get; set; }

    /// <summary>
    ///     Gets or sets invite date
    /// </summary>
    public DateTime? InviteDateUtc
    {
        get => _inviteDateUtc;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _inviteDateUtc, nameof(InviteDateUtc));
    }

    /// <summary>
    ///     Gets or sets content start nodes assigned to the User (not ones assigned to the user's groups)
    /// </summary>
    public int[] StartContentIds
    {
        get => _startContentIds;
        set
        {
            if (value == null)
            {
                value = new int[0];
            }

            BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startContentIds!, nameof(StartContentIds), _startIdsComparer);
        }
    }

    /// <summary>
    ///     Gets or sets media start nodes assigned to the User (not ones assigned to the user's groups)
    /// </summary>
    public int[] StartMediaIds
    {
        get => _startMediaIds;
        set
        {
            if (value == null)
            {
                value = new int[0];
            }

            BeingDirty.SetPropertyValueAndDetectChanges(value, ref _startMediaIds!, nameof(StartMediaIds), _startIdsComparer);
        }
    }

    /// <summary>
    ///     Gets a readonly list of the user's allowed sections which are based on it's user groups
    /// </summary>
    public string[] AllowedSections =>
        _allowedSections ??= _groups.SelectMany(x => x.AllowedSections).Distinct().ToArray();

    /// <summary>
    ///     Gets or sets the culture
    /// </summary>
    public string Culture
    {
        get => _culture;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _culture!, nameof(Culture));
    }

    public Guid Key => UserIdToInt(Id).ToGuid();

    /// <summary>
    ///     Used to construct a new instance without an identity
    /// </summary>
    /// <param name="email">This is allowed to be null (but would need to be filled in if trying to persist this instance)</param>
    public static BackOfficeIdentityUser CreateNew(GlobalSettings globalSettings, string? username, string email, string culture, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
        }

        var user = new BackOfficeIdentityUser(globalSettings, Array.Empty<IReadOnlyUserGroup>());
        user.DisableChangeTracking();
        user.UserName = username;
        user.Email = email;

        user.Id = string.Empty;
        user.HasIdentity = false;
        user._culture = culture;
        user.Name = name;
        user.EnableChangeTracking();
        return user;
    }

    /// <summary>
    ///     Gets or sets the user groups
    /// </summary>
    public void SetGroups(IReadOnlyCollection<IReadOnlyUserGroup> value)
    {
        // so they recalculate
        _allowedSections = null;

        _groups = value.ToArray();

        var roles = new List<IdentityUserRole<string>>();
        foreach (IdentityUserRole<string> identityUserRole in _groups.Select(x => new IdentityUserRole<string>
        {
            RoleId = x.Alias,
            UserId = Id,
        }))
        {
            roles.Add(identityUserRole);
        }

        // now reset the collection
        Roles = roles;
    }

    private static string UserIdToString(int userId) => string.Intern(userId.ToString(CultureInfo.InvariantCulture));

    private static int UserIdToInt(string? userId)
    {
        if (int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        if (Guid.TryParse(userId, out Guid key))
        {
            // Reverse the IntExtensions.ToGuid
            return BitConverter.ToInt32(key.ToByteArray(), 0);
        }

        throw new InvalidOperationException($"Unable to convert user ID ({userId})to int using InvariantCulture");
    }
}
